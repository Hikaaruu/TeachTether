using AutoMapper;
using TeachTether.Application.Interfaces.Repositories;

namespace TeachTether.Application.Services
{
    public class SchoolAdminService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public SchoolAdminService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

    }
}
