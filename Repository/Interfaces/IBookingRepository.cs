using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IBookingRepository:IRepository<Booking>
    {
        Task<List<Booking>> GetBookingsByOwner(int ownerId);
    }
}
