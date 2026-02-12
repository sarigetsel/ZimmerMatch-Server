using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZimmerMatch.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Repositories
{
    public class AvailabilityRepository : IRepository<Availability>
    {
        private readonly IContext ctx;

        public AvailabilityRepository(IContext context)
        {
            ctx = context;
        }
        public async Task<Availability> AddItem(Availability availability)
        {
            await ctx.Availabilities.AddAsync(availability);
            await ctx.Save();
            return availability;
        }

        public async Task DeleteItem(int id)
        {
            var av = await ctx.Availabilities.FirstOrDefaultAsync(a => a.AvailabilityId == id);
            ctx.Availabilities.Remove(av);
            await ctx.Save();
        }

        public async Task<List<Availability>> GetAll()
        {
            return await ctx.Availabilities.Include(a => a.Zimmers).ToListAsync();
        }

        public async Task<Availability> GetById(int id)
        {
            return await ctx.Availabilities.Include(a => a.Zimmers)
                .FirstOrDefaultAsync(x => x.AvailabilityId == id);
            
        }

        public async Task<Availability> UpdateItem(int id, Availability availability)
        {
            var av = await ctx.Availabilities.FirstOrDefaultAsync(a => a.AvailabilityId == id);
            if (av == null)
                return null;

            av.ZimmerId = availability.ZimmerId;
            av.EndTime = availability.EndTime;
            av.IsBooked = availability.IsBooked;

            await ctx.Save();
            return av;
        }
    }
}
