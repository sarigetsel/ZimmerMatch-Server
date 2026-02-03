using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common.Dto;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Services
{
    public class ZimmerService:IService<ZimmerDto>
    {
        private readonly IRepository<Zimmer> repository;
        private readonly IMapper mapper;

        public ZimmerService(IRepository<Zimmer> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<ZimmerDto> AddItem(ZimmerDto zimmerDto)
        {
            var created = await repository.AddItem(mapper.Map<Zimmer>(zimmerDto));
            return mapper.Map<ZimmerDto>(created);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<ZimmerDto>> GetAll()
        {
            var zimmers = await repository.GetAll();
            return mapper.Map<List<ZimmerDto>>(zimmers);
        }

        public async Task<ZimmerDto> GetById(int id)
        {
            var zimmer =await repository.GetById(id);
            return mapper.Map<ZimmerDto>(zimmer);
        }

        public async Task<ZimmerDto> UpdateItem(int id, ZimmerDto zimmerDto)
        {
            var updateZimmer = await repository.UpdateItem(id, mapper.Map<Zimmer>(zimmerDto));
            return mapper.Map<ZimmerDto>(updateZimmer);
        }
    }
}
