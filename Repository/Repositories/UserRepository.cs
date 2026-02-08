using ZimmerMatch.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class UserRepository:IRepository<User>
    {
        private readonly IContext ctx;

        public UserRepository(IContext context)
        {
            ctx = context;
        }
        public async Task<User> AddItem(User user)
        {
           await ctx.Users.AddAsync(user);
           await ctx.Save();
           return user;
        }

        public async Task DeleteItem(int id)
        {
            var user = await ctx.Users.FirstOrDefaultAsync(u => u.Id == id);
            ctx.Users.Remove(user);
            ctx.Save();
        }

        public async Task<List<User>> GetAll()
        {
           var x= ctx.Users.ToList();
            return await ctx.Users.ToListAsync();
        }

        public async Task<User> GetById(int id)
        {
            return await ctx.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> UpdateItem(int id, User user)
        {
            var us = await ctx.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (us == null)
                return null;
            us.Name = user.Name;
            us.Email = user.Email;
            us.Password = user.Password;
            us.Phone = user.Phone;
            us.Role = user.Role;
            await ctx.Save();
            return us;
        }
    }
}
