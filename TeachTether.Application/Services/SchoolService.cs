using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services;

public class SchoolService(IUnitOfWork unitOfWork, IMapper mapper, ISchoolDeletionHelper schoolDeletionHelper)
    : ISchoolService
{
    private readonly IMapper _mapper = mapper;
    private readonly ISchoolDeletionHelper _schoolDeletionHelper = schoolDeletionHelper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<SchoolResponse> CreateAsync(CreateSchoolRequest request, int schoolOwnerId)
    {
        var _ = await _unitOfWork.SchoolOwners.GetByIdAsync(schoolOwnerId)
                ?? throw new NotFoundException("School owner not found");

        if (await _unitOfWork.Schools.AnyAsync(s =>
                s.SchoolOwnerId == schoolOwnerId &&
                s.Name == request.Name))
            throw new BadRequestException($"You already have school with the name \"{request.Name}\".");

        var school = _mapper.Map<School>(request);
        school.SchoolOwnerId = schoolOwnerId;

        await _unitOfWork.Schools.AddAsync(school);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SchoolResponse>(school);
    }

    public async Task DeleteAsync(int id)
    {
        await _schoolDeletionHelper.DeleteSchoolAsync(id);
    }

    public async Task<IEnumerable<SchoolResponse>> GetAllByOwnerAsync(int schoolOwnerId)
    {
        var schools = await _unitOfWork.Schools.GetBySchoolOwnerIdAsync(schoolOwnerId);
        return _mapper.Map<IEnumerable<SchoolResponse>>(schools);
    }

    public async Task<SchoolResponse> GetByIdAsync(int id)
    {
        var school = await _unitOfWork.Schools.GetByIdAsync(id)
                     ?? throw new NotFoundException("School not found");
        return _mapper.Map<SchoolResponse>(school);
    }

    public async Task UpdateAsync(int id, UpdateSchoolRequest request)
    {
        var school = await _unitOfWork.Schools.GetByIdAsync(id)
                     ?? throw new NotFoundException("School not found");

        if (await _unitOfWork.Schools.AnyAsync(s =>
                s.SchoolOwnerId == school.SchoolOwnerId &&
                s.Name == request.Name))
            throw new BadRequestException($"You already have school with the name \"{request.Name}\".");

        _mapper.Map(request, school);
        _unitOfWork.Schools.Update(school);
        await _unitOfWork.SaveChangesAsync();
    }
}