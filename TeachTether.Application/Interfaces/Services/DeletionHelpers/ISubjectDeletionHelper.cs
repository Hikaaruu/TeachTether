namespace TeachTether.Application.Interfaces.Services.DeletionHelpers;

public interface ISubjectDeletionHelper
{
    Task DeleteSubjectAsync(int id);
}