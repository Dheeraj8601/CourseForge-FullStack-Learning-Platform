
using CourseForge.Domain;
using Microsoft.EntityFrameworkCore;

namespace CourseForge.Infrastructure;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // ==========================================
    // DbSets
    // ==========================================

    public DbSet<User> Users => Set<User>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<LessonProgress> Progress => Set<LessonProgress>();

    public DbSet<Purchase> Purchases => Set<Purchase>();

    public DbSet<Certificate> Certificates => Set<Certificate>();

    // ==========================================
    // Entity Configurations
    // ==========================================

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ==========================================
        // Unique Indexes
        // ==========================================

        b.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        b.Entity<Course>()
            .HasIndex(x => x.Slug)
            .IsUnique();

        b.Entity<Enrollment>()
            .HasIndex(x => new
            {
                x.UserId,
                x.CourseId
            })
            .IsUnique();

        b.Entity<LessonProgress>()
            .HasIndex(x => new
            {
                x.UserId,
                x.LessonId
            })
            .IsUnique();

        b.Entity<Certificate>()
            .HasIndex(x => new
            {
                x.UserId,
                x.CourseId
            })
            .IsUnique();

        b.Entity<Certificate>()
            .HasIndex(x => x.Code)
            .IsUnique();

        b.Entity<Purchase>()
            .HasIndex(x => x.RazorpayOrderId)
            .IsUnique();

        // ==========================================
        // Decimal Precision
        // ==========================================

        b.Entity<Course>()
            .Property(x => x.PriceInr)
            .HasPrecision(12, 2);

        b.Entity<Purchase>()
            .Property(x => x.AmountInr)
            .HasPrecision(12, 2);

        // ==========================================
        // Course → Lessons (One-to-Many)
        // ==========================================

        b.Entity<Lesson>()
            .HasOne(x => x.Course)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================================
        // Lesson → Questions (One-to-Many)
        // ==========================================

        b.Entity<Question>()
            .HasOne<Lesson>()
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
