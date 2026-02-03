using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.Dto;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Services
{
    public class AvailabilityService: IService<AvailabilityDto>
    { 
        private readonly IRepository<Availability> repository;
        private readonly IMapper mapper;

        public AvailabilityService(IRepository<Availability> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<AvailabilityDto> AddItem(AvailabilityDto availabilityDto)
        {
            var created = await repository.AddItem(mapper.Map<Availability>(availabilityDto));
            return  mapper.Map<AvailabilityDto>(created);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<AvailabilityDto>> GetAll()
        {
            var list = await repository.GetAll();
            return mapper.Map<List<AvailabilityDto>>(list);
        }

        public async Task<AvailabilityDto> GetById(int id)
        {
            var entity = await repository.GetById(id);
            return mapper.Map<AvailabilityDto>(entity);
        }

        public async Task<AvailabilityDto> UpdateItem(int id, AvailabilityDto availabilityDto)
        {
            var updatedEntity = await repository.UpdateItem(id, mapper.Map<Availability>(availabilityDto));
            return mapper.Map<AvailabilityDto>(updatedEntity);
        }
    }
}
