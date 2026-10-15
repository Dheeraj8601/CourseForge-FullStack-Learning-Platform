
using System.Security.Claims;
using System.Text.Json;
using CourseForge.Application;
using CourseForge.Domain;
using CourseForge.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseForge.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(AppDbContext db) : ControllerBase
{
    private Guid? Me()
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var courses = await db.Courses
                              .Where(x => x.Published)
                              .OrderBy(x => x.Title)
                              .Select(x => new
                              {
                                x.Id,
                                x.Title,
                                x.Summary,
                                x.Category,
                                x.CoverUrl,
                                x.PriceInr,
                                LessonCount = x.Lessons.Count
                              }).ToListAsync();

        return Ok(courses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var course = await db.Courses
            .Include(x => x.Lessons)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                (x.Published || User.IsInRole("Admin")));

        if (course == null)
        {
            return NotFound();
        }

        var me = Me();

        var enrolled = me.HasValue &&
            await db.Enrollments.AnyAsync(x =>
                x.CourseId == id &&
                x.UserId == me);

        return Ok(new
        {
            course.Id,
            course.Title,
            course.Summary,
            course.Category,
            course.CoverUrl,
            course.PriceInr,
            course.PassingPercent,
            course.SkipQuizzes,
            Enrolled = enrolled,
            Lessons = course.Lessons
                .OrderBy(x => x.Position)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Position
                })
        });
    }

    [Authorize]
    [HttpPost("{id:guid}/enroll")]
    public async Task<IActionResult> Enroll(Guid id)
    {
        var me = Me()!.Value;

        var course = await db.Courses
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Published);

        if (course == null)
        {
            return NotFound();
        }

        if (course.PriceInr > 0)
        {
            return BadRequest("Paid course: complete Razorpay checkout");
        }

        var alreadyEnrolled = await db.Enrollments.AnyAsync(x =>
            x.CourseId == id &&
            x.UserId == me);

        if (!alreadyEnrolled)
        {
            db.Enrollments.Add(new Enrollment
            {
                CourseId = id,
                UserId = me
            });

            await db.SaveChangesAsync();
        }

        return Ok(new
        {
            enrolled = true
        });
    }

    [Authorize]
    [HttpGet("{id:guid}/learn/{lessonId:guid}")]
    public async Task<IActionResult> Read(Guid id, Guid lessonId)
    {
        var me = Me()!.Value;

        var course = await db.Courses
            .Include(x => x.Lessons)
                .ThenInclude(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (course == null)
        {
            return NotFound();
        }

        var enrolled = await db.Enrollments.AnyAsync(x =>
            x.CourseId == id &&
            x.UserId == me);

        if (!enrolled)
        {
            return Forbid();
        }

        var orderedLessons = course.Lessons
                                .OrderBy(x => x.Position)
                                .ThenBy(x => x.Id)
                                .ToList();

        var index = orderedLessons.FindIndex(x => x.Id == lessonId);

        if (index < 0)
        {
            return NotFound();
        }

        if (index > 0)
        {
            var previousLessonId = orderedLessons[index - 1].Id;

            var previousPassed = await db.Progress.AnyAsync(x =>
                x.UserId == me &&
                x.LessonId == previousLessonId &&
                x.Passed);

            if (!previousPassed)
            {
                return StatusCode(423, new
                {
                    message = "Complete the previous lesson first"
                });
            }
        }

        var lesson = orderedLessons[index];

        var progress = await db.Progress
            .FirstOrDefaultAsync(x =>
                x.UserId == me &&
                x.LessonId == lessonId);

        return Ok(new
        {
            lesson.Id,
            lesson.Title,
            lesson.BodyMarkdown,
            lesson.Position,

            Passed = progress?.Passed ?? false,
            Attempts = progress?.Attempts ?? 0,

            Questions = lesson.Questions.Select(x => new
            {
                x.Id,
                x.Prompt,
                Options = JsonSerializer.Deserialize<List<string>>(
                    x.OptionsJson)
            }),

            NextLessonId = index + 1 < orderedLessons.Count
                ? orderedLessons[index + 1].Id
                : (Guid?)null
        });
    }

    [Authorize]
    [HttpPost("{id:guid}/learn/{lessonId:guid}/submit")]
    public async Task<IActionResult> Submit(
        Guid id,
        Guid lessonId,
        QuizSubmission request)
    {
        var me = Me()!.Value;

        var course = await db.Courses
            .Include(x => x.Lessons)
                .ThenInclude(x => x.Questions)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (course == null)
        {
            return NotFound();
        }

        var enrolled = await db.Enrollments.AnyAsync(x =>
            x.CourseId == id &&
            x.UserId == me);

        if (!enrolled)
        {
            return Forbid();
        }

        var orderedLessons = course.Lessons
            .OrderBy(x => x.Position)
            .ThenBy(x => x.Id)
            .ToList();

        var index = orderedLessons.FindIndex(x =>
            x.Id == lessonId);

        if (index < 0)
        {
            return NotFound();
        }

        if (index > 0)
        {
            var previousLessonId = orderedLessons[index - 1].Id;

            var previousPassed = await db.Progress.AnyAsync(x =>
                x.UserId == me &&
                x.LessonId == previousLessonId &&
                x.Passed);

            if (!previousPassed)
            {
                return StatusCode(
                    423,
                    "Previous lesson is not passed");
            }
        }

        var lesson = orderedLessons[index];

        var progress = await db.Progress
            .FirstOrDefaultAsync(x =>
                x.UserId == me &&
                x.LessonId == lessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                UserId = me,
                LessonId = lessonId
            };

            db.Progress.Add(progress);
        }

        if (progress.Passed)
        {
            return Ok(new
            {
                passed = true,
                percent = 100,
                attempts = progress.Attempts,
                alreadyPassed = true
            });
        }

        progress.Attempts++;

        var questionCount = lesson.Questions.Count;

        var correctAnswers = lesson.Questions.Count(q =>
            request.Answers.TryGetValue(
                q.Id,
                out var answer) &&
            answer == q.CorrectOption);

        var percent = questionCount == 0
            ? 0
            : (int)Math.Floor(
                correctAnswers * 100d / questionCount);

        var passed =
            course.SkipQuizzes ||
            questionCount == 0 ||
            percent >= course.PassingPercent;

        if (passed)
        {
            progress.Passed = true;
            progress.PassedUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        if (passed)
        {
            await IssueCertificateIfComplete(course, me);
        }

        return Ok(new
        {
            passed,
            percent,
            attempts = progress.Attempts,
            required = course.PassingPercent,

            answers = passed
                ? null
                : lesson.Questions.Select(q => new
                {
                    q.Id,
                    q.Explanation
                })
        });
    }

    private async Task IssueCertificateIfComplete(
        Course course,
        Guid userId)
    {
        var lessonIds = course.Lessons
            .Select(x => x.Id)
            .ToList();

        if (lessonIds.Count == 0)
        {
            return;
        }

        var passedCount = await db.Progress.CountAsync(x =>
            x.UserId == userId &&
            lessonIds.Contains(x.LessonId) &&
            x.Passed);

        if (passedCount != lessonIds.Count)
        {
            return;
        }

        var enrollment = await db.Enrollments.SingleAsync(x =>
            x.CourseId == course.Id &&
            x.UserId == userId);

        enrollment.CompletedUtc ??= DateTime.UtcNow;

        var certificateExists = await db.Certificates.AnyAsync(x =>
            x.CourseId == course.Id &&
            x.UserId == userId);

        if (!certificateExists)
        {
            db.Certificates.Add(new Certificate
            {
                CourseId = course.Id,
                UserId = userId,

                Code = "CF-" +
                    Guid.NewGuid()
                        .ToString("N")[..12]
                        .ToUpperInvariant()
            });
        }

        await db.SaveChangesAsync();
    }

    [Authorize]
    [HttpGet("my/learning")]
    public async Task<IActionResult> Mine()
    {
        var me = Me()!.Value;

        var enrollments = await db.Enrollments
            .Where(x => x.UserId == me)
            .ToListAsync();

        var enrolledCourseIds = enrollments
            .Select(x => x.CourseId)
            .ToList();

        var courses = await db.Courses
            .Include(x => x.Lessons)
            .Where(x => enrolledCourseIds.Contains(x.Id))
            .ToListAsync();

        var passedLessonIds = await db.Progress
            .Where(x =>
                x.UserId == me &&
                x.Passed)
            .Select(x => x.LessonId)
            .ToListAsync();

        var result = courses.Select(course => new
        {
            course.Id,
            course.Title,
            course.CoverUrl,
            course.Category,

            Total = course.Lessons.Count,

            Passed = course.Lessons.Count(lesson =>
                passedLessonIds.Contains(lesson.Id)),

            Completed = enrollments
                .First(e => e.CourseId == course.Id)
                .CompletedUtc != null
        });

        return Ok(result);
    }
}
