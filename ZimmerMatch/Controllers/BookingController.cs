using Common.Dto;
using Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.Security.Claims;

namespace ZimmerMatch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;
        private readonly IService<AvailabilityDto> _availabilityService;

        public BookingController(IBookingService service, IService<AvailabilityDto> availabilityService)
        {
            _service = service;
            _availabilityService = availabilityService;
        }

        // GET: api/Booking
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
                return StatusCode(500, "Failed to retrieve bookings.");
            }
        }

        // GET api/Booking/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var book = await _service.GetById(id);
                if (book == null) return NotFound();
                return Ok(book);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve booking.");
            }
        }

        [HttpGet("owner-bookings")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetBookingsByOwner()
        {
            try
            {
                var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(ownerIdClaim)) return Unauthorized();

                int ownerId = int.Parse(ownerIdClaim);
                var bookings = await _service.GetBookingsByOwner(ownerId);

                if (bookings == null || !bookings.Any())
                    return NotFound("No bookings found for this owner.");

                return Ok(bookings);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve owner bookings.");
            }
        }

        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

                int userId = int.Parse(userIdClaim);
                var bookings = await _service.GetAll();
                var myBookings = bookings.Where(b => b.UserId == userId).ToList();

                return Ok(myBookings ?? new List<BookingDto>());
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve your bookings.");
            }
        }

        // POST api/Booking
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BookingDto booking)
        {
            if (booking == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (!Enum.IsDefined(typeof(BookingStatus), booking.Status))
                    booking.Status = BookingStatus.Confirmed;

                var availabilities = (await _availabilityService.GetAll())
                    .Where(a => a.ZimmerId == booking.ZimmerId)
                    .ToList();

                for (var date = booking.StartDate.Date; date <= booking.EndDate.Date; date = date.AddDays(1))
                {
                    if (availabilities.Any(a => a.StartDate.Date == date && a.IsBooked))
                        return BadRequest($"Day {date:yyyy-MM-dd} is already booked.");
                }

                var createdBooking = await _service.AddItem(booking);

                for (var date = booking.StartDate.Date; date <= booking.EndDate.Date; date = date.AddDays(1))
                {
                    var existing = availabilities.FirstOrDefault(a => a.StartDate.Date == date);
                    if (existing != null)
                    {
                        existing.IsBooked = true;
                        await _availabilityService.UpdateItem(existing.AvailabilityId, existing);
                    }
                    else
                    {
                        await _availabilityService.AddItem(new AvailabilityDto
                        {
                            ZimmerId = booking.ZimmerId,
                            StartDate = date,
                            EndDate = date,
                            IsBooked = true,
                        });
                    }
                }

                var result = await _service.GetById(createdBooking.BookingId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your booking.");
            }
        }

        // PUT api/Booking/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] BookingDto booking)
        {
            if (booking == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _service.UpdateItem(id, booking);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch
            {
                return StatusCode(500, "Failed to update booking.");
            }
        }

        // DELETE api/Booking/5
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