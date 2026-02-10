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
        private readonly IService<AvailabilityDto> _service;

        public AvailabilityController(IService<AvailabilityDto> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var availabilities = await _service.GetAll();
                return Ok(availabilities);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve availabilities.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var availability = await _service.GetById(id);
                if (availability == null)
                    return NotFound();

                return Ok(availability);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve availability.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Post([FromBody] AvailabilityDto availability)
        {
            if (availability == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newAvailability = await _service.AddItem(availability);
                return Ok(newAvailability);
            }
            catch
            {
                return StatusCode(500, "Failed to create availability.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Put(int id,[FromBody] AvailabilityDto availability)
        {
            if (availability == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedAvailability = await _service.UpdateItem(id, availability);
                if (updatedAvailability == null)
                    return NotFound();

                return Ok(updatedAvailability);
            }
            catch
            {
                return StatusCode(500, "Failed to update availability.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteItem(id);
                return NoContent();
            }
            catch
            {
                return StatusCode(500, "Failed to delete availability.");
            }
        }
    }
}
