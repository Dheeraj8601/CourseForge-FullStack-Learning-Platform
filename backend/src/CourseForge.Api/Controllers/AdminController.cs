using System.Text.Json;
using System.Text.RegularExpressions;
using CourseForge.Application;
using CourseForge.Domain;
using CourseForge.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CourseForge.Api.Controllers;

[ApiController, Authorize(Roles = "Admin"), Route("api/admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("courses")]
    public async Task<IActionResult> Courses() =>
        Ok(await db.Courses.Include(x => x.Lessons).OrderByDescending(x => x.Title).Select(x => new
        {
            x.Id,
            x.Title,
            x.PriceInr,
            x.Published,
            x.SkipQuizzes,
            Lessons = x.Lessons.Count
        }).ToListAsync());

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse(CourseInput r)
    {
        if (string.IsNullOrWhiteSpace(r.Title) || r.PriceInr < 0 || r.PassingPercent < 1 || r.PassingPercent > 100)
            return BadRequest("Invalid course data");

        var slug = Regex.Replace(r.Title.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-') + "-" + Guid.NewGuid().ToString("N")[..6];
        var c = new Course
        {
            Title = r.Title,
            Slug = slug,
            Summary = r.Summary,
            Category = r.Category,
            CoverUrl = r.CoverUrl,
            PriceInr = r.PriceInr,
            Published = r.Published,
            SkipQuizzes = r.SkipQuizzes,
            PassingPercent = r.PassingPercent
        };

        db.Courses.Add(c);
        await db.SaveChangesAsync();
        return Ok(new { c.Id });
    }

    [HttpPut("courses/{id:guid}")]
    public async Task<IActionResult> EditCourse(Guid id, CourseInput r)
    {
        var c = await db.Courses.FindAsync(id);

        if (c == null) return NotFound();

        if (r.PriceInr < 0 || r.PassingPercent < 1 || r.PassingPercent > 100)
            return BadRequest();

        c.Title = r.Title;
        c.Summary = r.Summary;
        c.Category = r.Category;
        c.CoverUrl = r.CoverUrl;
        c.PriceInr = r.PriceInr;
        c.Published = r.Published;
        c.SkipQuizzes = r.SkipQuizzes;
        c.PassingPercent = r.PassingPercent;

        await db.SaveChangesAsync();

        return Ok();
    }
    [HttpPost("courses/{courseId:guid}/lessons")]
    public async Task<IActionResult> CreateLesson(Guid courseId, LessonInput r)
    {
        if (!await db.Courses.AnyAsync(x => x.Id == courseId))
            return NotFound();

        if (string.IsNullOrWhiteSpace(r.Title))
            return BadRequest();

        var l = new Lesson
        {
            CourseId = courseId,
            Title = r.Title,
            BodyMarkdown = r.BodyMarkdown,
            Position = r.Position
        };

        db.Lessons.Add(l);

        await db.SaveChangesAsync();
        return Ok(new { l.Id });
    }
    [HttpPut("lessons/{lessonId:guid}")]
    public async Task<IActionResult> EditLesson(Guid lessonId, LessonInput r)
    {
        var l = await db.Lessons.FindAsync(lessonId);

        if (l == null)
            return NotFound();

        l.Title = r.Title;

        l.BodyMarkdown = r.BodyMarkdown;
        l.Position = r.Position;

        await db.SaveChangesAsync();
        return Ok();
    }
    [HttpPost("lessons/{lessonId:guid}/questions")] 
    public async Task<IActionResult> CreateQuestion(Guid lessonId, QuestionInput r) 
    { 
        if (!await db.Lessons.AnyAsync(x => x.Id == lessonId)) 
            return NotFound(); 
        
        if (r.Options.Count < 2 || r.CorrectOption < 0 || r.CorrectOption >= r.Options.Count) 
            return BadRequest("Provide at least two options and a valid correct index"); 
        
        var q = new Question 
        { 
            LessonId = lessonId, 
            Prompt = r.Prompt, 
            OptionsJson = JsonSerializer.Serialize(r.Options), 
            CorrectOption = r.CorrectOption, 
            Explanation = r.Explanation 
        }; 
        
        db.Questions.Add(q); 
        await db.SaveChangesAsync(); 
        return Ok(new { q.Id }); 
    }
    [HttpDelete("questions/{questionId:guid}")] 
    public async Task<IActionResult> DeleteQuestion(Guid questionId) 
    { 
        var q = await db.Questions.FindAsync(questionId); 
        
        if (q == null) 
            return NotFound(); 
        
        db.Questions.Remove(q); 
        
        await db.SaveChangesAsync(); 
        return NoContent();
    }
    [HttpGet("stats")] public async Task<IActionResult> Stats() => Ok(new { courses = await db.Courses.CountAsync(), students = await db.Users.CountAsync(x => x.Role == "Student"), enrollments = await db.Enrollments.CountAsync(), certificates = await db.Certificates.CountAsync() });
}
