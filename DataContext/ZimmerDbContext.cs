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

            // 1. הגדרת הקשר בין הזמנה (Booking) לצימר
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Zimmer)
                .WithMany(z => z.Bookings)
                .HasForeignKey(b => b.ZimmerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. הגדרת הקשר בין הזמנה (Booking) למשתמש (User)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. הגדרת הקשר בין צימר לבעלים (User)
            modelBuilder.Entity<Zimmer>()
                .HasOne(z => z.Owner)
                .WithMany(u => u.Zimmers)
                .HasForeignKey(z => z.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. הגדרת הקשר בין זמינות (Availability) לצימר
            modelBuilder.Entity<Availability>()
                .HasOne(a => a.Zimmers)
                .WithMany(z => z.Availabilities)
                .HasForeignKey(a => a.ZimmerId);
        }

    }

}