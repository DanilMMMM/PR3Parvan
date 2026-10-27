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

        // For matching (сопоставление)
        public List<string> MatchingTerms { get; set; } = new List<string>();
        public List<string> MatchingDefinitions { get; set; } = new List<string>();
        public List<string> SelectedMatches { get; set; } = new List<string>();
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
        public List<MatchingResultPair> MatchingPairs { get; set; } = new List<MatchingResultPair>();
    }

    public class MatchingResultPair
    {
        public string Term { get; set; } = string.Empty;
        public string SelectedDefinition { get; set; } = string.Empty;
        public string CorrectDefinition { get; set; } = string.Empty;
        public bool IsMatchCorrect { get; set; }
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

    public class AcademicJournalViewModel
    {
        public int? SelectedDisciplineId { get; set; }
        public int? SelectedGroupId { get; set; }
        public List<Discipline> Disciplines { get; set; } = new List<Discipline>();
        public List<StudentGroup> Groups { get; set; } = new List<StudentGroup>();
        public List<Lecture> DisciplineLectures { get; set; } = new List<Lecture>();
        public List<JournalStudentMatrixRow> MatrixRows { get; set; } = new List<JournalStudentMatrixRow>();
        public List<JournalDetailEntry> DetailEntries { get; set; } = new List<JournalDetailEntry>();
        public JournalStats Stats { get; set; } = new JournalStats();
    }

    public class JournalStats
    {
        public int TotalStudentsCount { get; set; }
        public int TotalPassedAttempts { get; set; }
        public double AveragePassRate { get; set; }
        public int TotalLecturesCount { get; set; }
    }

    public class JournalStudentMatrixRow
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public int CompletedCount { get; set; }
        public int TotalLectures { get; set; }
        public int ProgressPercent { get; set; }
        public double AverageScorePercent { get; set; }
        public Dictionary<int, JournalCell> LectureCells { get; set; } = new Dictionary<int, JournalCell>();
    }

    public class JournalCell
    {
        public int LectureId { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsUnlockedManually { get; set; }
        public int? ScorePercent { get; set; }
        public int AttemptsCount { get; set; }
        public DateTime? LastAttemptDate { get; set; }
        public string Grade5Scale => ScorePercent switch
        {
            >= 85 => "5 (Отл)",
            >= 70 => "4 (Хор)",
            >= 50 => "3 (Удовл)",
            > 0 => "2 (Неуд)",
            _ => "—"
        };
    }

    public class JournalDetailEntry
    {
        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string DisciplineTitle { get; set; } = string.Empty;
        public string LectureTitle { get; set; } = string.Empty;
        public int LectureOrderIndex { get; set; }
        public string AccessStatus { get; set; } = string.Empty;
        public int? ScorePercent { get; set; }
        public string Grade5Scale { get; set; } = "—";
        public int AttemptsCount { get; set; }
        public int MaxAttempts { get; set; } = 3;
        public DateTime? ActivityDate { get; set; }
        public bool IsPassed { get; set; }
    }

    public class LectureRosterViewModel
    {
        public Lecture Lecture { get; set; } = null!;
        public Discipline Discipline { get; set; } = null!;
        public int? SelectedGroupId { get; set; }
        public List<StudentGroup> Groups { get; set; } = new List<StudentGroup>();
        public List<LectureRosterStudentRow> StudentRows { get; set; } = new List<LectureRosterStudentRow>();
        public int TotalEnrolled { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalInProgress { get; set; }
        public int TotalLocked { get; set; }
        public double PassRatePercent { get; set; }
    }

    public class LectureRosterStudentRow
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsUnlockedManually { get; set; }
        public int AttemptsCount { get; set; }
        public int? BestScorePercent { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public string Grade5Scale { get; set; } = "—";
    }
}
