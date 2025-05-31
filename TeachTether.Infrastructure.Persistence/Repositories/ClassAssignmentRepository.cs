using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories;

public class ClassAssignmentRepository(ApplicationDbContext context)
    : Repository<ClassAssignment>(context), IClassAssignmentRepository
{
    public async Task<IEnumerable<ClassAssignment>> GetByClassGroupSubjectIdAsync(int classGroupSubjectId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ca => ca.ClassGroupSubjectId == classGroupSubjectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClassAssignment>> GetByTeacherIdAsync(int teacherId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ca => ca.TeacherId == teacherId)
            .ToListAsync();
    }
}