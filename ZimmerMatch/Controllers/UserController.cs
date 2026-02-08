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

namespace ZimmerMatch.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly IService<UserDto> service;
        private readonly IsExist< UserDto> isExist;

        public UserController(IConfiguration configuration, IService<UserDto> service, IsExist<UserDto> isExist)
        {
            _configuration = configuration;
            this.service = service;
            this.isExist = isExist;
        }

        [HttpPost("login")]
         public async Task<string> Login([FromBody] Login l)
         {
            var user = await isExist.Exist(l);
             if (user != null)
                  return GenerateToken(user);
             return "user dosent exist..";
         }

        [HttpGet]
        public async Task<List<UserDto>> Get()
        {
            return await service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<UserDto> Get(int id)
        {
            return await service.GetById(id);
        }

        [HttpPost]
        public async Task<UserDto> Post([FromBody] UserDto user)
        {
            return await service.AddItem(user);
        }

        [HttpPut("{id}")]
        public async Task<UserDto> Put(int id,[FromBody] UserDto user)
        {
            return await service.UpdateItem(id,user);
        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await service.DeleteItem(id);
        }

        private string GenerateToken(UserDto u)
        {
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, u.Name),
                new Claim(ClaimTypes.Role,u.Role.ToString())
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
                Name = identity.FindFirst(ClaimTypes.Name)?.Value,
                Role = Enum.Parse<UserRole>(identity.FindFirst(ClaimTypes.Role)?.Value)
            };
       
        }
    }
}
