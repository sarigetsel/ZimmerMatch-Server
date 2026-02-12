using AutoMapper;
using Common.Dto;
using Microsoft.AspNetCore.Identity;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class UserService: IService<UserDto>, IsExist<UserDto>
    {
        private readonly IRepository<User> repository;
        private readonly IMapper mapper;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserService(IRepository<User> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<UserDto> AddItem(UserDto userDto)
        {
            var user = mapper.Map<User>(userDto);
            user.Password = _passwordHasher.HashPassword(user, user.Password);

            var created = await repository.AddItem(user);
            return mapper.Map<UserDto>(created);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<UserDto>> GetAll()
        {
            var users = await repository.GetAll();
            var userDto = mapper.Map<List<UserDto>>(users);
            foreach (var user in userDto)
            {
                user.Password = null;
            }
            return userDto;
        }

        public async Task<UserDto> GetById(int id)
        {
            var user = await repository.GetById(id);
            var userDto = mapper.Map<UserDto>(user);
            userDto.Password = null;
            return userDto;
        }

        public async Task<UserDto> UpdateItem(int id, UserDto userDto)
        {
            var userEntity = mapper.Map<User>(userDto);
            if (!string.IsNullOrEmpty(userEntity.Password))
            {
                userEntity.Password = _passwordHasher.HashPassword(userEntity, userEntity.Password);
            }

            var updatedUser = await repository.UpdateItem(id, userEntity);
            
            if(updatedUser == null)
                return null;

           var userDtoUpdated = mapper.Map<UserDto>(updatedUser);
            userDtoUpdated.Password = null;
            return userDtoUpdated;
        }
        public async Task<UserDto> Exist(Login l)
        {
            var user = (await repository.GetAll()).FirstOrDefault(u => u.Email == l.Email);
            if (user == null)
                return null;
            
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, l.Password);
           
            if (result == PasswordVerificationResult.Success)
                return mapper.Map<UserDto>(user);

            return null;
        }
       
    }
}

  