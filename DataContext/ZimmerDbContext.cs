using ZimmerMatch.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using System.Threading.Tasks;              

namespace ZimmerMatch
{
    public class ZimmerDbContext : DbContext, IContext
    {
        private readonly string? _connection;

        public ZimmerDbContext(string connectionString)
        {
            _connection = connectionString;
        }
        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Zimmer> Zimmers { get; set; }

        public virtual DbSet<Availability> Availabilities { get; set; }
        public Task Save()
        {
            return SaveChangesAsync();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseSqlServer(_connection);
        }

    }

}