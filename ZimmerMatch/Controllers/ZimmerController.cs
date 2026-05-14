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
                var zimmers = await _service.GetAll();

                var result = zimmers.Where(a => a.ZimmerId == zimmerId);

                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve zimmer.");
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
                return StatusCode(403, "אינך יכול להוסיף צימר עבור משתמש אחר!");
            }

            zimmer.OwnerId = currentUserId;

            if (zimmer == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "images");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                zimmer.ArrImages = new List<byte[]>();
                zimmer.ImageUrls = new List<string>();

                if (zimmer.ImageFiles != null && zimmer.ImageFiles.Any())
                {
                    foreach (var file in zimmer.ImageFiles)
                    {
                        if (file.Length == 0) continue;

                        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var imagesPath = Path.Combine(imagesDir, uniqueFileName);

                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            byte[] fileBytes = ms.ToArray();

                            await System.IO.File.WriteAllBytesAsync(imagesPath, fileBytes);

                            zimmer.ArrImages.Add(fileBytes);

                            zimmer.ImageUrls.Add(uniqueFileName);
                        }
                    }
                }

                var newZimmer = await _service.AddItem(zimmer);
                return Ok(newZimmer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Post Error]: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Put(int id, [FromForm] ZimmerDto zimmer)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);
            var existingZimmer = await _service.GetById(id);
            if (existingZimmer == null) return NotFound();

            if (existingZimmer.OwnerId != currentUserId)
                return Forbid("אינך יכול לעדכן צימר שאינו שייך לך!");

            zimmer.OwnerId = currentUserId;
            zimmer.ZimmerId = id;

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var imagesDir = Path.Combine(Environment.CurrentDirectory, "images");
                if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);

                if (zimmer.ImageFiles != null && zimmer.ImageFiles.Any())
                {
                    zimmer.ArrImages = new List<byte[]>();
                    foreach (var file in zimmer.ImageFiles)
                    {
                        if (file.Length == 0) continue;
                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        byte[] fileBytes = ms.ToArray();
                        zimmer.ArrImages.Add(fileBytes);

                        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var imagesPath = Path.Combine(imagesDir, uniqueFileName);
                        await System.IO.File.WriteAllBytesAsync(imagesPath, fileBytes);
                    }
                }

                var updatedZimmer = await _service.UpdateItem(id, zimmer);
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
                    Console.WriteLine(ex.Message);
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
