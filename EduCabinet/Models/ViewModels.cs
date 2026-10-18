using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduCabinet.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [MinLength(3, ErrorMessage = "Логин должен быть не короче 3 символов")]
        [Display(Name = "Логин")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите ФИО")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не короче 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Роль")]
        public string Role { get; set; } = UserRoles.Student;

        [Display(Name = "Учебная группа (для студента)")]
        public int? GroupId { get; set; }
    }

    public class LectureDetailViewModel
    {
        public Lecture Lecture { get; set; } = null!;
        public bool IsUnlocked { get; set; }
        public string? LockReason { get; set; }
        public bool IsCompleted { get; set; }
        public Test? Test { get; set; }
        public TestAttempt? BestAttempt { get; set; }
        public int RemainingAttempts { get; set; }
        public int PreviousLectureId { get; set; }
        public int NextLectureId { get; set; }
        public bool HasControlQuestions { get; set; }
    }

    public class TestTakeViewModel
    {
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public int LectureId { get; set; }
        public string LectureTitle { get; set; } = string.Empty;
        public int PassingScorePercent { get; set; }
        public int MaxAttempts { get; set; }
        public int CurrentAttemptNumber { get; set; }
        public int TimeLimitMinutes { get; set; }
        public List<QuestionTakeItem> Questions { get; set; } = new List<QuestionTakeItem>();
    }

    public class QuestionTakeItem
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "SingleChoice";
        public int Points { get; set; }
        public List<OptionTakeItem> Options { get; set; } = new List<OptionTakeItem>();
        
        // Student answers to bind
        public int? SelectedOptionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new List<int>();
        public string? TextAnswer { get; set; }
    }

    public class OptionTakeItem
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }

    public class TestResultViewModel
    {
        public int AttemptId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public int LectureId { get; set; }
        public string LectureTitle { get; set; } = string.Empty;
        public int DisciplineId { get; set; }
        public int ScorePercent { get; set; }
        public int EarnedPoints { get; set; }
        public int TotalPoints { get; set; }
        public int PassingScorePercent { get; set; }
        public bool IsPassed { get; set; }
        public int AttemptNumber { get; set; }
        public int RemainingAttempts { get; set; }
        public bool NextLectureUnlocked { get; set; }
        public int? NextLectureId { get; set; }
        public List<QuestionResultItem> Questions { get; set; } = new List<QuestionResultItem>();
    }

    public class QuestionResultItem
    {
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string UserAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
    }

    public class StudentDashboardViewModel
    {
        public User Student { get; set; } = null!;
        public string GroupName { get; set; } = "Не назначена";
        public List<DisciplineProgressItem> Disciplines { get; set; } = new List<DisciplineProgressItem>();
        public List<TestAttempt> RecentAttempts { get; set; } = new List<TestAttempt>();
    }

    public class DisciplineProgressItem
    {
        public int DisciplineId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalLectures { get; set; }
        public int CompletedLectures { get; set; }
        public int ProgressPercent { get; set; }
        public List<TopicItemViewModel> Topics { get; set; } = new List<TopicItemViewModel>();
    }

    public class TopicItemViewModel
    {
        public int TopicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<LectureItemViewModel> Lectures { get; set; } = new List<LectureItemViewModel>();
    }

    public class LectureItemViewModel
    {
        public int LectureId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public int? BestScorePercent { get; set; }
        public bool HasTest { get; set; }
    }

    public class TeacherDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalDisciplines { get; set; }
        public int TotalLectures { get; set; }
        public int TotalAttempts { get; set; }
        public List<Discipline> Disciplines { get; set; } = new List<Discipline>();
        public List<StudentGroup> Groups { get; set; } = new List<StudentGroup>();
        public List<StudentProgressRow> StudentsProgress { get; set; } = new List<StudentProgressRow>();
    }

    public class StudentProgressRow
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string DisciplineTitle { get; set; } = string.Empty;
        public int CompletedLectures { get; set; }
        public int TotalLectures { get; set; }
        public int OverallProgressPercent { get; set; }
        public double AverageScorePercent { get; set; }
        public DateTime? LastActivityAt { get; set; }
    }
}
