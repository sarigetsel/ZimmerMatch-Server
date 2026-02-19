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
    [Route("api/[Controller]")]
    [ApiController]
    public class ZimmerController:ControllerBase
    {
        private readonly IService<ZimmerDto> _service;

        public ZimmerController(IService<ZimmerDto> service)
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

        [HttpPost]

        // הרשאת גישה רק לבעל הצימר
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Post([FromForm] ZimmerDto zimmer)
        {
            // 1. שליפת ה-ID של המשתמש מהטוקן
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);

            if (zimmer.OwnerId != 0 && zimmer.OwnerId != currentUserId)
            {
                return Forbid("אינך יכול להוסיף צימר עבור משתמש אחר!");
            }

            zimmer.OwnerId = currentUserId;
            // 2. השמה כפויה של ה-ID לתוך הצימר - כך שגם אם הוא שלח OwnerId אחר, הוא יידרס
            zimmer.OwnerId = int.Parse(userIdClaim.Value);


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
        public async Task<IActionResult> Put(int id,[FromForm] ZimmerDto zimmer)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);

            if (zimmer.OwnerId != 0 && zimmer.OwnerId != currentUserId)
            {
                return Forbid("אינך יכול לעדכן צימר שאינו שייך לך!");
            }

            zimmer.OwnerId = currentUserId;
            zimmer.OwnerId = int.Parse(userIdClaim.Value);


            if (zimmer == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedZimmer = await _service.UpdateItem(id, zimmer);
                if (updatedZimmer == null)
                    return NotFound();

                return Ok(updatedZimmer);
            }
            catch
            {
                return StatusCode(500, "Failed to update zimmer.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        // רק מנהל יכול למחוק צימרים
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteItem(id);
                return NoContent();
            }
            catch
            {
                return StatusCode(500, "Failed to delete zimmer.");
            }
        }
    }
}
