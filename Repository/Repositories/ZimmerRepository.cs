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
    public class ZimmerRepository : IRepository<Zimmer>
    {
        private readonly IContext ctx;

        public ZimmerRepository(IContext context)
        {
            ctx = context;
        }
        public async Task<Zimmer> AddItem(Zimmer zimmer)
        {
            await ctx.Zimmers.AddAsync(zimmer);
            await ctx.Save();
            return zimmer;
        }

        public async Task DeleteItem(int id)
        {
            var zimmer =await ctx.Zimmers.FirstOrDefaultAsync(z => z.ZimmerId == id);
            ctx.Zimmers.Remove(zimmer);
            ctx.Save();
        }

        public async Task<List<Zimmer>> GetAll()
        {
            return await ctx.Zimmers.ToListAsync();
        }

        public async Task<Zimmer> GetById(int id)
        {
            return await ctx.Zimmers.FirstOrDefaultAsync(z => z.ZimmerId == id);
        }

        public async Task<Zimmer> UpdateItem(int id, Zimmer zimmer)
        {
            var zi = await ctx.Zimmers.FirstOrDefaultAsync(z => z.ZimmerId == id);
            if (zi == null)
                return null;
            if (zimmer.OwnerId > 0)
            {
                zi.OwnerId = zimmer.OwnerId;
            }
            zi.NameZimmer = zimmer.NameZimmer;
            zi.Description = zimmer.Description;
            zi.City = zimmer.City; 
            zi.Address = zimmer.Address; 
            zi.Latitude = zimmer.Latitude; 
            zi.Longitude = zimmer.Longitude; 
            zi.NumRooms = zimmer.NumRooms; 
            zi.PricePerNight = zimmer.PricePerNight;
            zi.Facilities = zimmer.Facilities;
            if(zimmer.ImageUrls != null && zimmer.ImageUrls.Any())
                zi.ImageUrls = new List<string>(zimmer.ImageUrls);
            await ctx.Save();
            return zi;
        }
    }
}
