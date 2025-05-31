namespace TeachTether.Application.DTOs;

public class ClassAssignmentResponse
{
    public int ClassGroupId { get; set; }
    public required string ClassGroupName { get; set; }
    public int SubjectId { get; set; }
    public required string SubjectName { get; set; }
}