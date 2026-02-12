using AutoMapper;
using Common.Dto;
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
    public class BookingService :IBookingService
    {
        private readonly IBookingRepository repository;
        private readonly IMapper mapper;
        public BookingService(IBookingRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<BookingDto> AddItem(BookingDto bookingDto)
        {
            var entity = mapper.Map<Booking>(bookingDto);
            var createdEntity = await repository.AddItem(entity);
            return mapper.Map<BookingDto>(createdEntity);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<BookingDto>> GetAll()
        {
            var list = await repository.GetAll();
            return mapper.Map<List<BookingDto>>(list);
        }

        public async Task<List<BookingDto>> GetBookingsByOwner(int ownerId)
        {
            var bookings = await repository.GetBookingsByOwner(ownerId);
            return mapper.Map<List<BookingDto>>(bookings);
        }

        public async Task<BookingDto> GetById(int id)
        {
            var entity = await repository.GetById(id);
            return mapper.Map<BookingDto>(entity);
        }

        public async Task<BookingDto> UpdateItem(int id, BookingDto booking)
        {
            var updateBooking = await repository.UpdateItem(id, mapper.Map<Booking>(booking));
            return mapper.Map<BookingDto>(updateBooking);
        }

        
    }
}
