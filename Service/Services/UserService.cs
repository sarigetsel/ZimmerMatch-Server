using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common.Dto;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Services
{
    public class UserService:IService<UserDto>,IsExist<UserDto>
    {
        private readonly IRepository<User> repository;
        private readonly IMapper mapper;

        public UserService(IRepository<User> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<UserDto> AddItem(UserDto userDto)
        {
            var created = await repository.AddItem(mapper.Map<User>(userDto));
            return mapper.Map<UserDto>(created);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

       
      

        public async Task<List<UserDto>> GetAll()
        {
            var users = await repository.GetAll();
            return mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> GetById(int id)
        {
            var user = await repository.GetById(id);
            return mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateItem(int id, UserDto userDto)
        {
            var updateUser = await repository.UpdateItem(id, mapper.Map<User>(userDto));
            return mapper.Map<UserDto>(updateUser);
        }
        public async Task<UserDto> Exist(Login l)
        {
            var user = (await repository.GetAll()).FirstOrDefault(u => u.Email == l.Email && u.Password == l.Password);
            if (user != null)
                return mapper.Map<UserDto>(user);
            return null;
        }

    }
}

  