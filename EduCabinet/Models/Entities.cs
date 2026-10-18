using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCabinet.Models
{
    public static class UserRoles
    {
        public const string Teacher = "Teacher";
        public const string Student = "Student";
    }

    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = UserRoles.Student;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<GroupMembership> GroupMemberships { get; set; } = new List<GroupMembership>();
        public ICollection<LectureProgress> Progresses { get; set; } = new List<LectureProgress>();
        public ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
    }

    [Table("StudentGroups")]
    public class StudentGroup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<GroupMembership> Memberships { get; set; } = new List<GroupMembership>();
    }

    [Table("GroupMemberships")]
    public class GroupMembership
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public User Student { get; set; } = null!;

        public int GroupId { get; set; }
        [ForeignKey(nameof(GroupId))]
        public StudentGroup Group { get; set; } = null!;
    }

    [Table("Disciplines")]
    public class Discipline
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int CreatedByTeacherId { get; set; }
        [ForeignKey(nameof(CreatedByTeacherId))]
        public User CreatedByTeacher { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Topic> Topics { get; set; } = new List<Topic>();
    }

    [Table("Topics")]
    public class Topic
    {
        [Key]
        public int Id { get; set; }

        public int DisciplineId { get; set; }
        [ForeignKey(nameof(DisciplineId))]
        public Discipline Discipline { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public int OrderIndex { get; set; } = 1;

        public ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();
    }

    [Table("Lectures")]
    public class Lecture
    {
        [Key]
        public int Id { get; set; }

        public int TopicId { get; set; }
        [ForeignKey(nameof(TopicId))]
        public Topic Topic { get; set; } = null!;

        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(200)]
        public string? OriginalFileName { get; set; }

        public int OrderIndex { get; set; } = 1;

        public int MinPassingScore { get; set; } = 70; // 70% passing requirement

        public ICollection<ControlQuestion> ControlQuestions { get; set; } = new List<ControlQuestion>();
        public ICollection<Test> Tests { get; set; } = new List<Test>();
        public ICollection<LectureProgress> Progresses { get; set; } = new List<LectureProgress>();
    }

    [Table("ControlQuestions")]
    public class ControlQuestion
    {
        [Key]
        public int Id { get; set; }

        public int LectureId { get; set; }
        [ForeignKey(nameof(LectureId))]
        public Lecture Lecture { get; set; } = null!;

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string QuestionType { get; set; } = "TextAnswer"; // "TextAnswer", "SingleChoice", "MultipleChoice"

        public string CorrectAnswer { get; set; } = string.Empty;

        public int OrderIndex { get; set; } = 1;
    }

    [Table("Tests")]
    public class Test
    {
        [Key]
        public int Id { get; set; }

        public int LectureId { get; set; }
        [ForeignKey(nameof(LectureId))]
        public Lecture Lecture { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public int PassingScorePercent { get; set; } = 70;

        public int MaxAttempts { get; set; } = 3;

        public int TimeLimitMinutes { get; set; } = 15;

        public ICollection<TestQuestion> Questions { get; set; } = new List<TestQuestion>();
        public ICollection<TestAttempt> Attempts { get; set; } = new List<TestAttempt>();
    }

    [Table("TestQuestions")]
    public class TestQuestion
    {
        [Key]
        public int Id { get; set; }

        public int TestId { get; set; }
        [ForeignKey(nameof(TestId))]
        public Test Test { get; set; } = null!;

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [MaxLength(50)]
        public string QuestionType { get; set; } = "SingleChoice"; // "SingleChoice", "MultipleChoice", "TextAnswer"

        public int Points { get; set; } = 1;

        public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
    }

    [Table("AnswerOptions")]
    public class AnswerOption
    {
        [Key]
        public int Id { get; set; }

        public int QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public TestQuestion Question { get; set; } = null!;

        [Required]
        public string OptionText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; } = false;
    }

    [Table("TestAttempts")]
    public class TestAttempt
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public User Student { get; set; } = null!;

        public int TestId { get; set; }
        [ForeignKey(nameof(TestId))]
        public Test Test { get; set; } = null!;

        public int AttemptNumber { get; set; } = 1;

        public int ScorePercent { get; set; } = 0;

        public int TotalPoints { get; set; } = 0;

        public int EarnedPoints { get; set; } = 0;

        public bool IsPassed { get; set; } = false;

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public ICollection<StudentAnswer> Answers { get; set; } = new List<StudentAnswer>();
    }

    [Table("StudentAnswers")]
    public class StudentAnswer
    {
        [Key]
        public int Id { get; set; }

        public int AttemptId { get; set; }
        [ForeignKey(nameof(AttemptId))]
        public TestAttempt Attempt { get; set; } = null!;

        public int QuestionId { get; set; }
        [ForeignKey(nameof(QuestionId))]
        public TestQuestion Question { get; set; } = null!;

        public string? SelectedOptionIdsJson { get; set; }

        public string? TextAnswer { get; set; }

        public bool IsCorrect { get; set; } = false;
    }

    [Table("LectureProgress")]
    public class LectureProgress
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public User Student { get; set; } = null!;

        public int LectureId { get; set; }
        [ForeignKey(nameof(LectureId))]
        public Lecture Lecture { get; set; } = null!;

        public bool IsCompleted { get; set; } = false;

        public bool IsUnlockedManually { get; set; } = false;

        public DateTime? UnlockedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int? TeacherId { get; set; }
        [ForeignKey(nameof(TeacherId))]
        public User? Teacher { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
