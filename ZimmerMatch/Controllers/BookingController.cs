using Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Service.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ZimmerMatch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;
        
        public BookingController(IBookingService service)
        {
            _service = service;
        }


        // GET: api/<BookingController>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get()
        {
            try
            {
                 var book = await _service.GetAll();
                 return Ok(book);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve booking.");
            }
            
        }

        // GET api/<BookingController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                  var book = await _service.GetById(id);
                   if(book == null)
                   {
                       return NotFound();
                   }
                   return Ok(book);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve booking.");
            }
        }

        [HttpGet("owner/{ownerId}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetBookingsByOwner(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest("Invalid owner id.");
            try
            {
                var bookings = await _service.GetBookingsByOwner(ownerId);

                if (bookings == null || !bookings.Any())
                    return NotFound("No bookings found for this owner.");

                return Ok(bookings);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve bookings for owner.");
            }
        }

        // POST api/<BookingController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BookingDto booking)
        {
            if (booking == null || !ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                 var book = await _service.AddItem(booking);
                return Ok(book);
            }
            catch
            {
                return StatusCode(500, "Failed to create booking.");
            }

        }

        // PUT api/<BookingController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] BookingDto booking)
        {
            if (booking == null || !ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var book = await _service.UpdateItem(id, booking);
                if ( book == null)
                {
                    return NotFound();
                }
                return Ok(book);
            }
            catch
            {
                return StatusCode(500, "Failed to update booking.");
            }
        }

        // DELETE api/<BookingController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteItem(id);
                return NoContent();
            }
            catch
            {
                return StatusCode(500, "Failed to delete booking.");

            }
        }
    }
}
