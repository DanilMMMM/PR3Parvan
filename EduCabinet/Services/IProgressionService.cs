using System.Threading.Tasks;
using System.Collections.Generic;
using EduCabinet.Models;

namespace EduCabinet.Services
{
    public interface IProgressionService
    {
        Task<(bool CanAccess, string? Reason)> CheckLectureAccessAsync(int studentId, int lectureId);
        Task<bool> CompleteLectureAfterTestAsync(int studentId, int testId, int attemptId);
        Task<int> CalculateDisciplineProgressPercentAsync(int studentId, int disciplineId);
        Task<List<LectureItemViewModel>> GetLecturesWithStatusForStudentAsync(int studentId, int disciplineId);
        Task<bool> ManuallyUnlockLectureAsync(int studentId, int lectureId, int teacherId, string reason);
        Task<bool> ResetStudentTestAttemptsAsync(int studentId, int testId, int teacherId);
    }
}
