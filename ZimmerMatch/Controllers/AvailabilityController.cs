using Common.Dto;
using ZimmerMatch.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repository.Entities;
using Service.Interfaces;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ZimmerMatch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IService<AvailabilityDto> service;

        public AvailabilityController(IService<AvailabilityDto> service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<List<AvailabilityDto>> Get()
        {
            return await service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<AvailabilityDto> Get(int id)
        {
            return await service.GetById(id);
        }

        [HttpPost]
        public async Task<AvailabilityDto> Post(AvailabilityDto availability)
        {
            return await service.AddItem(availability);
        }

        [HttpPut("{id}")]
        public async Task<AvailabilityDto> Put(int id,[FromBody] AvailabilityDto availability)
        {
            return await service.UpdateItem(id, availability);
        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await service.DeleteItem(id);
        }
    }
}
