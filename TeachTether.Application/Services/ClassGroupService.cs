using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class ClassGroupService : IClassGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClassGroupDeletionHelper _deletionHelper;

        public ClassGroupService(IUnitOfWork unitOfWork, IMapper mapper, IClassGroupDeletionHelper deletionHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _deletionHelper = deletionHelper;
        }

        public async Task<ClassGroupResponse> CreateAsync(CreateClassGroupRequest request, int schoolId)
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(request.HomeroomTeacherId);

            if (teacher is null || teacher.SchoolId != schoolId)
                throw new NotFoundException("Teacher not found");

            if (await _unitOfWork.ClassGroups.AnyAsync(cg => 
                cg.SchoolId == schoolId && 
                cg.GradeYear == request.GradeYear && 
                cg.Section == request.Section))
            {
                throw new BadRequestException($"Class group {request.GradeYear} - {request.Section} already exists in school {schoolId}.");
            }

            var classGroup = _mapper.Map<ClassGroup>(request);
            classGroup.SchoolId = schoolId;

            await _unitOfWork.ClassGroups.AddAsync(classGroup);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClassGroupResponse>(classGroup);
        }

        public async Task DeleteAsync(int id)
        {
            await _deletionHelper.DeleteClassGroupAsync(id);
        }

        public async Task<IEnumerable<ClassGroupResponse>> GetAllBySchoolAsync(int schoolId)
        {
            var classGroups = await _unitOfWork.ClassGroups.GetBySchoolIdAsync(schoolId);
            return _mapper.Map<IEnumerable<ClassGroupResponse>>(classGroups);
        }

        public async Task<IEnumerable<ClassGroupResponse>> GetAllByTeacherAsync(int teacherId)
        {
            var classGroups = await _unitOfWork.ClassGroups.GetByHomeroomTeacherIdAsync(teacherId);
            return _mapper.Map<IEnumerable<ClassGroupResponse>>(classGroups);
        }

        public async Task<IEnumerable<ClassGroupResponse>> GetAvailableForTeacherAsync(int teacherId)
        {
            var clsGrpSubjIds = (await _unitOfWork.ClassAssignments.GetByTeacherIdAsync(teacherId))
                .Select(ca => ca.ClassGroupSubjectId);
            var classGroupIds = (await _unitOfWork.ClassGroupsSubjects.GetByIdsAsync(clsGrpSubjIds)).
                Select(cgs => cgs.ClassGroupId);
            var homeClassGroupIds = (await _unitOfWork.ClassGroups.GetByHomeroomTeacherIdAsync(teacherId))
                .Select(cg => cg.Id);
            var allClassGroupIds = classGroupIds.Union(homeClassGroupIds);
            var classGroups = await _unitOfWork.ClassGroups.GetByIdsAsync(allClassGroupIds);
            return _mapper.Map<IEnumerable<ClassGroupResponse>>(classGroups);
        }

        public async Task<ClassGroupResponse> GetByIdAsync(int id)
        {
            var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(id)
                ?? throw new NotFoundException("Class Group not found");

            return _mapper.Map<ClassGroupResponse>(classGroup);
        }

        public async Task<ClassGroupResponse> GetByStudentAsync(int studentId)
        {
            var classGroupStudent = await _unitOfWork.ClassGroupStudents.GetByStudentIdAsync(studentId)
                ?? throw new NotFoundException("Student is not in class group");

            var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(classGroupStudent.ClassGroupId)
                ?? throw new NotFoundException("Class Group not found");

            return _mapper.Map<ClassGroupResponse>(classGroup);
        }

        public async Task UpdateAsync(int id, UpdateClassGroupRequest request)
        {
            var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(id)
                 ?? throw new NotFoundException("Class Group not found");

            if ((classGroup.Section != request.Section || classGroup.GradeYear!= request.GradeYear) && 
                await _unitOfWork.ClassGroups.AnyAsync(cg =>
                cg.SchoolId == classGroup.SchoolId &&
                cg.GradeYear == request.GradeYear &&
                cg.Section == request.Section))
            {
                throw new BadRequestException($"Class group {request.GradeYear} - {request.Section} already exists in school {classGroup.SchoolId}.");
            }

            _mapper.Map(request, classGroup);
            _unitOfWork.ClassGroups.Update(classGroup);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
