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
using Common.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ZimmerMatch.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly IService<UserDto> _service;
        private readonly IsExist< UserDto> _isExist;

        public UserController(IConfiguration configuration, IService<UserDto> service, IsExist<UserDto> isExist)
        {
            _configuration = configuration;
            _service = service;
            _isExist = isExist;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login l)
        {
            if(l == null || string.IsNullOrWhiteSpace(l.Email) || string.IsNullOrWhiteSpace(l.Password))
                return BadRequest("Email and password are required.");

            try
            {
                var user = await _isExist.Exist(l);
                if (user != null)
                     return Ok(new { Token = GenerateToken(user) });

                return NotFound("User doesnt exist.");
            }
            catch
            {
                return StatusCode(500, "An unexpected error occurred during login.");
            }
         }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var users = await _service.GetAll();
                users.ForEach(u => u.Password = null);
                return Ok(users);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve users.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null || currentUser.Id != id)
                    return Forbid();

                var user = await _service.GetById(id);
                if (user == null)
                    return NotFound();

                user.Password = null; // Hide password
                return Ok(user);
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve user.");
            }
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserDto user)
        {
            if (user == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var exists = (await _service.GetAll()).Any(u => u.Email == user.Email);
                if (exists)
                    return BadRequest("User already exists.");

                var newUser = await _service.AddItem(user);
                var token = GenerateToken(newUser);

                return Ok(new { User = newUser, Token = token });
            }
            catch
            {
                return StatusCode(500, "Failed to create user.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserDto user)
        {
            if (user == null || !ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var updatedUser = await _service.UpdateItem(id, user);
                updatedUser.Password = null;
                return Ok(updatedUser);
            }
            catch
            {
                return StatusCode(500, "Failed to update user.");
            }
        }

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
                return StatusCode(500, "Failed to delete user.");
            }
        }


        private string GenerateToken(UserDto u)
        {
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, u.Name),
                new Claim(ClaimTypes.Role,u.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, u.Id.ToString())
            };
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private UserDto GetCurrentUser()
        {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null) return null;

            return new UserDto()
            {
                Id = int.Parse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                Name = identity.FindFirst(ClaimTypes.Name)?.Value,
                Role = Enum.Parse<UserRole>(identity.FindFirst(ClaimTypes.Role)?.Value)
            };
       
        }
    }
}
