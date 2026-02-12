using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZimmerMatch.Interfaces;

namespace Repository.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly IContext ctx;
        public BookingRepository(IContext context)
        {
            ctx = context;
        }
        public async Task<Booking> AddItem(Booking booking)
        {
            ctx.Bookings.AddAsync(booking);
            await ctx.Save();
            return booking;
        }

        public async Task DeleteItem(int id)
        {
            var b = await ctx.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
            ctx.Bookings.Remove(b);
            await ctx.Save();
        }

        public async Task<List<Booking>> GetAll()
        {
            return await ctx.Bookings
                      .Include(b => b.User)   
                      .Include(b => b.Zimmer) 
                      .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByOwner(int ownerId)
        {
           return await ctx.Bookings
                .Include(b => b.Zimmer)
                .Where(b => b.Zimmer.OwnerId == ownerId)
                .Include(b => b.User)
                .ToListAsync();
        }

        public async Task<Booking> GetById(int id)
        {
            return await ctx.Bookings
                     .Include(b => b.User)   
                     .Include(b => b.Zimmer)
                     .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<Booking> UpdateItem(int id, Booking booking)
        {
            var book = await ctx.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
            if (book == null)
                return null;

            book.UserId = booking.UserId;
            book.ZimmerId = booking.ZimmerId;
            book.StartDate = booking.StartDate;
            book.EndDate = booking.EndDate;
            book.NumGuests = booking.NumGuests;
            book.TotalPrice = booking.TotalPrice;
            book.Status = booking.Status;
            book.SpecialRequests = booking.SpecialRequests;

            await ctx.Save();
            return book;
        }
    }
}
