namespace TeachTether.Application.Interfaces.Services
{
    public interface IStudentRecordsService
    {
        Task DeleteForClassGroupSubject(int classGroupId, int subjectId);
    }
}
