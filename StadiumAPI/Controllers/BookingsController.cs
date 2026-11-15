using Microsoft.AspNetCore.Mvc;
using StadiumReservationApi.DTOs;
using StadiumReservationApi.Services;

namespace StadiumReservationApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly BookingService _service;

    public BookingsController(BookingService service)
    {
        _service = service;
    }

    [HttpGet("stadium/{stadiumId}")]
    public async Task<IActionResult> GetStadiumBookings(int stadiumId, [FromQuery] bool includeCancelled = false)
    {
        var bookings = await _service.GetStadiumBookingsAsync(stadiumId, includeCancelled);
        return Ok(bookings);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        var (booking, error) = await _service.CreateBookingAsync(dto);
        return error.HasValue ? ErrorResponse(error.Value) : Ok(booking);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBooking(int id)
    {
        var booking = await _service.GetBookingAsync(id);
        return booking == null ? NotFound() : Ok(booking);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBooking(int id, [FromBody] CreateBookingDto dto)
    {
        var (booking, error) = await _service.UpdateBookingAsync(id, dto);
        return error.HasValue ? ErrorResponse(error.Value) : Ok(booking);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var error = await _service.CancelBookingAsync(id);
        return error.HasValue ? ErrorResponse(error.Value) : NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBooking(int id) =>
        await _service.DeleteBookingAsync(id) ? NoContent() : NotFound();

    private IActionResult ErrorResponse(BookingError error)
    {
        return error switch
        {
            BookingError.BookingNotFound => NotFound("Booking not found."),
            BookingError.Cancelled => Conflict("Cancelled bookings cannot be edited."),
            BookingError.InvalidDetails => BadRequest("Enter a customer name and an end time after the start."),
            BookingError.CancellationDeadline => BadRequest("The cancellation deadline for this stadium has passed."),
            BookingError.StadiumNotFound => NotFound("الملعب غير موجود"),
            BookingError.StadiumClosed => BadRequest("الملعب مغلق حالياً للصيانة"),
            BookingError.TimeConflict => BadRequest("عذراً، هذا التوقيت محجوز مسبقاً! يرجى اختيار موعد آخر."),
            _ => throw new InvalidOperationException($"Unexpected booking error: {error}")
        };
    }
}
