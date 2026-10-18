using Microsoft.EntityFrameworkCore;
using EduCabinet.Models;

namespace EduCabinet.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
        public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();
        public DbSet<Discipline> Disciplines => Set<Discipline>();
        public DbSet<Topic> Topics => Set<Topic>();
        public DbSet<Lecture> Lectures => Set<Lecture>();
        public DbSet<ControlQuestion> ControlQuestions => Set<ControlQuestion>();
        public DbSet<Test> Tests => Set<Test>();
        public DbSet<TestQuestion> TestQuestions => Set<TestQuestion>();
        public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
        public DbSet<TestAttempt> TestAttempts => Set<TestAttempt>();
        public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();
        public DbSet<LectureProgress> LectureProgresses => Set<LectureProgress>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraint on Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Discipline -> CreatedByTeacher
            modelBuilder.Entity<Discipline>()
                .HasOne(d => d.CreatedByTeacher)
                .WithMany()
                .HasForeignKey(d => d.CreatedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Topic -> Discipline
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Discipline)
                .WithMany(d => d.Topics)
                .HasForeignKey(t => t.DisciplineId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lecture -> Topic
            modelBuilder.Entity<Lecture>()
                .HasOne(l => l.Topic)
                .WithMany(t => t.Lectures)
                .HasForeignKey(l => l.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            // ControlQuestion -> Lecture
            modelBuilder.Entity<ControlQuestion>()
                .HasOne(cq => cq.Lecture)
                .WithMany(l => l.ControlQuestions)
                .HasForeignKey(cq => cq.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Test -> Lecture
            modelBuilder.Entity<Test>()
                .HasOne(t => t.Lecture)
                .WithMany(l => l.Tests)
                .HasForeignKey(t => t.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            // TestQuestion -> Test
            modelBuilder.Entity<TestQuestion>()
                .HasOne(tq => tq.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(tq => tq.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            // AnswerOption -> TestQuestion
            modelBuilder.Entity<AnswerOption>()
                .HasOne(ao => ao.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(ao => ao.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // TestAttempt -> Student
            modelBuilder.Entity<TestAttempt>()
                .HasOne(ta => ta.Student)
                .WithMany(u => u.TestAttempts)
                .HasForeignKey(ta => ta.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // TestAttempt -> Test
            modelBuilder.Entity<TestAttempt>()
                .HasOne(ta => ta.Test)
                .WithMany(t => t.Attempts)
                .HasForeignKey(ta => ta.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentAnswer -> TestAttempt
            modelBuilder.Entity<StudentAnswer>()
                .HasOne(sa => sa.Attempt)
                .WithMany(a => a.Answers)
                .HasForeignKey(sa => sa.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // LectureProgress -> Student
            modelBuilder.Entity<LectureProgress>()
                .HasOne(lp => lp.Student)
                .WithMany(u => u.Progresses)
                .HasForeignKey(lp => lp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // LectureProgress -> Lecture
            modelBuilder.Entity<LectureProgress>()
                .HasOne(lp => lp.Lecture)
                .WithMany(l => l.Progresses)
                .HasForeignKey(lp => lp.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            // LectureProgress unique (StudentId, LectureId)
            modelBuilder.Entity<LectureProgress>()
                .HasIndex(lp => new { lp.StudentId, lp.LectureId })
                .IsUnique();

            // GroupMembership unique (StudentId, GroupId)
            modelBuilder.Entity<GroupMembership>()
                .HasIndex(gm => new { gm.StudentId, gm.GroupId })
                .IsUnique();
        }
    }
}
