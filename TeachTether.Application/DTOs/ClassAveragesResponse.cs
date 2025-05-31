namespace TeachTether.Application.DTOs;

public class ClassAveragesResponse
{
    public decimal? GradeAverage { get; set; }
    public decimal? BehaviorAverage { get; set; }

    public AttendanceBreakdown Attendance { get; set; } = new();
}

public class AttendanceBreakdown
{
    public decimal PresentPercentage { get; set; }
    public decimal LatePercentage { get; set; }
    public decimal AbsentPercentage { get; set; }
    public decimal ExcusedPercentage { get; set; }
}