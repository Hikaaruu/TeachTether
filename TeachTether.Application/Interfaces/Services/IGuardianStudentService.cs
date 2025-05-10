namespace TeachTether.Application.Interfaces.Services
{
    public interface IGuardianStudentService
    {
        Task CreateAsync(int studentId, int guardianId);
        Task DeleteAsync(int studentId, int guardianId);
    }
}
