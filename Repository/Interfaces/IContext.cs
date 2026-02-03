using Microsoft.EntityFrameworkCore;
using Repository.Entities;

namespace ZimmerMatch.Interfaces
{
    public interface IContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Zimmer> Zimmers { get; set; }

        public DbSet<Availability> Availabilities { get; set; }
        public Task Save();
        
    }
}
