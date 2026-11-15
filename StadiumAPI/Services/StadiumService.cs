using Microsoft.EntityFrameworkCore;
using StadiumReservationApi.Data;
using StadiumReservationApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace StadiumReservationApi.Services
{
    public class StadiumService
    {
        private readonly AppDbContext _context;

        public StadiumService(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<Stadium>> GetAllStadiumsAsync() => _context.Stadiums.AsNoTracking()
            .Include(s => s.BookingPolicy).OrderBy(s => s.Id).ToListAsync();
        public ValueTask<Stadium?> GetStadiumByIdAsync(int id) => _context.Stadiums.FindAsync(id);

        public Task<List<Stadium>> SearchStadiumsAsync(string keyword)
        {
            return _context.Stadiums
                .Where(s => s.Name.Contains(keyword) || s.Location.Contains(keyword))
                .ToListAsync();
        }

        public Task<List<Stadium>> GetPagedStadiumsAsync(int pageIndex, int pageSize)
        {
            return _context.Stadiums
                .OrderBy(s => s.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddStadiumAsync(Stadium stadium)
        {
            _context.Stadiums.Add(stadium);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStadiumAsync(Stadium updatedStadium)
        {
            var stadium = await _context.Stadiums.Include(s => s.BookingPolicy)
                .FirstOrDefaultAsync(s => s.Id == updatedStadium.Id);

            if (stadium != null)
            {
                stadium.Name = updatedStadium.Name;
                stadium.Location = updatedStadium.Location;
                stadium.PricePerHour = updatedStadium.PricePerHour;
                stadium.CategoryId = updatedStadium.CategoryId;
                stadium.IsAvailable = updatedStadium.IsAvailable;
                if (updatedStadium.BookingPolicy is { } policy)
                {
                    stadium.BookingPolicy ??= new StadiumBookingPolicy();
                    stadium.BookingPolicy.OpeningTime = policy.OpeningTime;
                    stadium.BookingPolicy.ClosingTime = policy.ClosingTime;
                    stadium.BookingPolicy.SlotDurationMinutes = policy.SlotDurationMinutes;
                    stadium.BookingPolicy.MaxAdvanceBookingDays = policy.MaxAdvanceBookingDays;
                    stadium.BookingPolicy.CancellationDeadlineHours = policy.CancellationDeadlineHours;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task ToggleAvailabilityAsync(int id)
        {
            var stadium = await _context.Stadiums.FindAsync(id);
            if (stadium != null)
            {
                stadium.IsAvailable = !stadium.IsAvailable;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteStadiumAsync(int id)
        {
            var stadium = await _context.Stadiums.FindAsync(id);
            if (stadium != null)
            {
                _context.Stadiums.Remove(stadium);
                await _context.SaveChangesAsync();
            }
        }
    }
}
