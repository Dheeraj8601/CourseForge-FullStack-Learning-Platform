
namespace CourseForge.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string? PasswordHash { get; set; }

    public string? GoogleSubject { get; set; }

    public string Role { get; set; } = "Student";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

public class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "";

    public string Slug { get; set; } = "";

    public string Summary { get; set; } = "";

    public string Category { get; set; } = "Technology";

    public string CoverUrl { get; set; } = "";

    public decimal PriceInr { get; set; } = 0;

    public bool Published { get; set; } = true;

    public bool SkipQuizzes { get; set; } = false;

    public int PassingPercent { get; set; } = 70;

    public List<Lesson> Lessons { get; set; } = new();
}

public class Lesson
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CourseId { get; set; }

    public Course? Course { get; set; }

    public int Position { get; set; }

    public string Title { get; set; } = "";

    public string BodyMarkdown { get; set; } = "";

    public List<Question> Questions { get; set; } = new();
}

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LessonId { get; set; }

    public string Prompt { get; set; } = "";

    public string OptionsJson { get; set; } = "[]";

    public int CorrectOption { get; set; } = 0;

    public string Explanation { get; set; } = "";
}

public class Enrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }

    public DateTime EnrolledUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedUtc { get; set; }
}

public class LessonProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid LessonId { get; set; }

    public int Attempts { get; set; } = 0;

    public bool Passed { get; set; } = false;

    public DateTime? PassedUtc { get; set; }
}

public class Purchase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }

    public decimal AmountInr { get; set; }

    public string RazorpayOrderId { get; set; } = "";

    public string? RazorpayPaymentId { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

public class Certificate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }

    public string Code { get; set; } = "";

    public DateTime IssuedUtc { get; set; } = DateTime.UtcNow;
}
