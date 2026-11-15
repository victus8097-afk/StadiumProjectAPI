using Microsoft.EntityFrameworkCore;
using StadiumReservationApi.Data;
using StadiumReservationApi.DTOs;
using StadiumReservationApi.Models;

namespace StadiumReservationApi.Services;

public enum BookingError { StadiumNotFound, StadiumClosed, TimeConflict, BookingNotFound, Cancelled, InvalidDetails, CancellationDeadline }

public class BookingService                                                                   
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)        
    {
        _context = context;
    }

    public Task<List<Booking>> GetStadiumBookingsAsync(int stadiumId, bool includeCancelled = false)
    {
        return _context.Bookings.AsNoTracking()
            .Where(b => b.StadiumId == stadiumId && (includeCancelled || b.Status == "Confirmed"))
            .OrderBy(b => b.StartTime)
            .ToListAsync();
    }

    public Task<Booking?> GetBookingAsync(int id) =>
        _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    public Task<(Booking? Booking, BookingError? Error)> CreateBookingAsync(CreateBookingDto dto) =>
        SaveBookingAsync(null, dto);

    public Task<(Booking? Booking, BookingError? Error)> UpdateBookingAsync(int id, CreateBookingDto dto) =>
        SaveBookingAsync(id, dto);

    private async Task<(Booking? Booking, BookingError? Error)> SaveBookingAsync(int? id, CreateBookingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerName) || dto.EndTime <= dto.StartTime)
            return (null, BookingError.InvalidDetails);

        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var booking = id.HasValue ? await _context.Bookings.FindAsync(id.Value) : new Booking();
        if (booking == null) return (null, BookingError.BookingNotFound);
        if (booking.Status == "Cancelled") return (null, BookingError.Cancelled);

        var stadium = await _context.Stadiums.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.StadiumId);
        if (stadium == null) return (null, BookingError.StadiumNotFound);
        if (!stadium.IsAvailable) return (null, BookingError.StadiumClosed);

        var conflict = await _context.Bookings.AnyAsync(b => b.Id != booking.Id &&
            b.StadiumId == dto.StadiumId && b.Status == "Confirmed" &&
            b.StartTime < dto.EndTime && dto.StartTime < b.EndTime);
        if (conflict) return (null, BookingError.TimeConflict);

        if (!id.HasValue || booking.StadiumId != dto.StadiumId ||
            booking.StartTime != dto.StartTime || booking.EndTime != dto.EndTime)
            booking.TotalPrice = (decimal)(dto.EndTime - dto.StartTime).TotalHours * stadium.PricePerHour;

        booking.CustomerName = dto.CustomerName.Trim();
        booking.CustomerPhone = dto.CustomerPhone.Trim();
        booking.StartTime = dto.StartTime;
        booking.EndTime = dto.EndTime;
        booking.StadiumId = dto.StadiumId;
        if (!id.HasValue) _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return (booking, null);
    }

    public async Task<BookingError?> CancelBookingAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return BookingError.BookingNotFound;
        if (booking.Status == "Cancelled") return null;
        var hours = await _context.StadiumBookingPolicies.Where(p => p.StadiumId == booking.StadiumId)
            .Select(p => p.CancellationDeadlineHours).FirstOrDefaultAsync();
        if ((booking.StartTime - DateTime.Now).TotalHours < hours)
            return BookingError.CancellationDeadline;
        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return null;
    }

    public async Task<bool> DeleteBookingAsync(int id) =>
        await _context.Bookings.Where(b => b.Id == id).ExecuteDeleteAsync() > 0;
}
