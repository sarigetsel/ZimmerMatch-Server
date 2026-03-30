using Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using Repository.Entities;
using Service.Interfaces;
using Service.Services;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZimmerMatch.Interfaces;

namespace ZimmerMatch.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class ZimmerController:ControllerBase
    {
        private readonly IZimmerService _service;

        public ZimmerController(IZimmerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var zimmers = await _service.GetAll();
                return Ok(zimmers);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve zimmers.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var zimmer = await _service.GetById(id);
                if (zimmer == null)
                    return NotFound();

                return Ok(zimmer);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve zimmer.");
            }
        }
        [HttpGet("zimmer/{zimmerId}")]
        public async Task<IActionResult> GetByZimmer(int zimmerId)
        {
            try
            {
                var availabilities = await _service.GetAll();

                var result = availabilities.Where(a => a.ZimmerId == zimmerId);

                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve availability.");
            }
        }


        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Post([FromForm] ZimmerDto zimmer)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);

            if (zimmer.OwnerId != 0 && zimmer.OwnerId != currentUserId)
            {
                return Forbid("אינך יכול להוסיף צימר עבור משתמש אחר!");
            }

            zimmer.OwnerId = currentUserId;

            if (zimmer == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var imagesDir = Path.Combine(Environment.CurrentDirectory, "images");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                zimmer.ArrImages = new List<byte[]>();

                foreach (var file in zimmer.ImageFiles)
                {
                    if (file.Length == 0)
                        continue;

                    var imagesPath = Path.Combine(imagesDir, file.FileName);
                    using (var fs = new FileStream(imagesPath, FileMode.Create))
                        await file.CopyToAsync(fs);

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    zimmer.ArrImages.Add(ms.ToArray());
                }

                var newZimmer = await _service.AddItem(zimmer);
                return Ok(newZimmer);
            }
            catch
            {
                return StatusCode(500, "Failed to create zimmer.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Put(int id, [FromForm] ZimmerDto zimmer)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);

            if (zimmer.OwnerId != 0 && zimmer.OwnerId != currentUserId)
                return Forbid("אינך יכול לעדכן צימר שאינו שייך לך!");

            zimmer.OwnerId = currentUserId;

            if (zimmer == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existingZimmer = await _service.GetById(id);
                if (existingZimmer == null)
                    return NotFound();

                var imagesDir = Path.Combine(Environment.CurrentDirectory, "images");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                if (existingZimmer.ArrImages != null)
                    existingZimmer.ArrImages.Clear();

                zimmer.ArrImages = new List<byte[]>();

                foreach (var file in zimmer.ImageFiles)
                {
                    if (file.Length == 0)
                        continue;

                    var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var imagesPath = Path.Combine(imagesDir, uniqueFileName);

                    using var ms = new MemoryStream();
                    await file.OpenReadStream().CopyToAsync(ms);
                    zimmer.ArrImages.Add(ms.ToArray());
                    await System.IO.File.WriteAllBytesAsync(imagesPath, ms.ToArray());
                }

                var updatedZimmer = await _service.UpdateItem(id, zimmer);
                if (updatedZimmer == null)
                    return NotFound();

                return Ok(updatedZimmer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, "Failed to update zimmer.");
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")] 
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingZimmer = await _service.GetById(id);
                if (existingZimmer == null)
                    return NotFound("הצימר לא נמצא.");

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized();

                int currentUserId = int.Parse(userIdClaim.Value);
                bool isAdmin = User.IsInRole("Admin");

                if (!isAdmin && existingZimmer.OwnerId != currentUserId)
                {
                    return Forbid("אינך מורשה למחוק צימר שאינו בבעלותך!");
                }

                await _service.DeleteItem(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "נכשלה מחיקת הצימר.");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<ZimmerDto>>> Search([FromQuery] ZimmerSearchDto searchParams)
        {
            try
            {
                var results = await _service.SearchZimmersAsync(searchParams);
                return Ok(results);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }
        [HttpGet("cities")]
        public async Task<ActionResult<List<string>>> GetCities()
        {
            var zimmers = await _service.GetAll();

            var cities = zimmers
                .Where(z => !string.IsNullOrEmpty(z.City))
                .Select(z => z.City.Trim())
                .Distinct()
                .OrderBy(c => c) 
                .ToList();

            return Ok(cities);
        }
    }
}
