using FluentAssertions;
using Moq;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class StudentRecordsServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IClassGroupStudentRepository> _mockCgsRepo = new();
    private readonly Mock<IStudentGradeRepository> _mockGradeRepo = new();
    private readonly Mock<IStudentBehaviorRepository> _mockBehaviorRepo = new();
    private readonly Mock<IStudentAttendanceRepository> _mockAttendanceRepo = new();
    private readonly StudentRecordsService _sut;

    public StudentRecordsServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.ClassGroupStudents).Returns(_mockCgsRepo.Object);
        _mockUnitOfWork.Setup(u => u.StudentGrades).Returns(_mockGradeRepo.Object);
        _mockUnitOfWork.Setup(u => u.StudentBehaviors).Returns(_mockBehaviorRepo.Object);
        _mockUnitOfWork.Setup(u => u.StudentAttendances).Returns(_mockAttendanceRepo.Object);
        _sut = new StudentRecordsService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task DeleteForClassGroupSubject_WhenCalled_ShouldDeleteAllRecordsAndSave()
    {
        // Arrange
        var classGroupStudents = new List<ClassGroupStudent>
        {
            new() { StudentId = 1, ClassGroupId = 10 },
            new() { StudentId = 2, ClassGroupId = 10 }
        };
        var grades = new List<StudentGrade>
        {
            new() { Id = 1, StudentId = 1, SubjectId = 5 }
        };
        var behaviors = new List<StudentBehavior>
        {
            new() { Id = 1, StudentId = 2, SubjectId = 5 }
        };
        var attendances = new List<StudentAttendance>
        {
            new() { Id = 1, StudentId = 1, SubjectId = 5 }
        };

        _mockCgsRepo.Setup(r => r.GetByClassGroupIdAsync(10)).ReturnsAsync(classGroupStudents);
        _mockGradeRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StudentGrade, bool>>>()))
            .ReturnsAsync(grades);
        _mockBehaviorRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StudentBehavior, bool>>>()))
            .ReturnsAsync(behaviors);
        _mockAttendanceRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StudentAttendance, bool>>>()))
            .ReturnsAsync(attendances);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.DeleteForClassGroupSubject(classGroupId: 10, subjectId: 5);

        // Assert
        _mockGradeRepo.Verify(r => r.DeleteMany(grades), Times.Once);
        _mockBehaviorRepo.Verify(r => r.DeleteMany(behaviors), Times.Once);
        _mockAttendanceRepo.Verify(r => r.DeleteMany(attendances), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteForClassGroupSubject_WhenNoStudentsInClassGroup_ShouldStillSave()
    {
        // Arrange
        _mockCgsRepo.Setup(r => r.GetByClassGroupIdAsync(10)).ReturnsAsync(new List<ClassGroupStudent>());
        _mockGradeRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StudentGrade, bool>>>()))
            .ReturnsAsync(new List<StudentGrade>());
        _mockBehaviorRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StudentBehavior, bool>>>()))
            .ReturnsAsync(new List<StudentBehavior>());
        _mockAttendanceRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StudentAttendance, bool>>>()))
            .ReturnsAsync(new List<StudentAttendance>());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.DeleteForClassGroupSubject(classGroupId: 10, subjectId: 5);

        // Assert
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
