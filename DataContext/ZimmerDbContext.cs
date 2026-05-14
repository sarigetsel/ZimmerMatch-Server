using ZimmerMatch.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using System.Threading.Tasks;              

namespace ZimmerMatch
{
    public class ZimmerDbContext : DbContext, IContext
    {
        public ZimmerDbContext() { }

        public ZimmerDbContext(DbContextOptions<ZimmerDbContext> options) : base(options) { }

        private readonly string? _connection;

        public ZimmerDbContext(string connectionString)
        {
            _connection = connectionString;
        }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Zimmer> Zimmers { get; set; }

        public virtual DbSet<Availability> Availabilities { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }
        public Task Save()
        {
            return  SaveChangesAsync();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connection);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Zimmer)
                .WithMany(z => z.Bookings)
                .HasForeignKey(b => b.ZimmerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Zimmer>()
                .HasOne(z => z.Owner)
                .WithMany(u => u.Zimmers)
                .HasForeignKey(z => z.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Availability>()
                .HasOne(a => a.Zimmers)
                .WithMany(z => z.Availabilities)
                .HasForeignKey(a => a.ZimmerId);
        }

    }

}