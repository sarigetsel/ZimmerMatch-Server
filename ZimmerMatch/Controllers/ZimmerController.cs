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
        private readonly IService<ZimmerDto> service;

        public ZimmerController(IService<ZimmerDto> service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<List<ZimmerDto>> Get()
        {
            return await service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<ZimmerDto> Get(int id)
        {
            return await service.GetById(id);
        }

        [HttpPost]

        // הרשאת גישה רק לבעל הצימר
        //[Authorize(Roles = "Owner")]
        public async Task<ZimmerDto> Post([FromForm] ZimmerDto zimmer)
        {
            var imagesPath = Path.Combine(Environment.CurrentDirectory, "images/", zimmer.ImageFiles.FileName);
            using (FileStream fs = new FileStream(imagesPath, FileMode.Create))
            {
                zimmer.ImageFiles.CopyTo(fs);
                fs.Close();
            }
            return await service.AddItem(zimmer);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Owner")]
        public async Task<ZimmerDto> Put(int id,[FromForm] ZimmerDto zimmer)
        {
            return await service.UpdateItem(id,zimmer);
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "Owner")]
        public async Task Delete(int id)
        {
             await service.DeleteItem(id);
        }
    }
}
