
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IAnalyticsService
    {
        Task<ClassAveragesResponse> GetClassAverages(int subjectId, int studentId, int classGroupId);
    }
}
