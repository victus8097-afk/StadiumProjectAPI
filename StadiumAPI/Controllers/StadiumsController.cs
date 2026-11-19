using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using StadiumReservationApi.Models;
using StadiumReservationApi.DTOs; 
using StadiumReservationApi.Services;

namespace StadiumReservationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StadiumsController : ControllerBase
    {
        private readonly StadiumService _service;

        public StadiumsController(StadiumService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<Stadium>>> Get() => Ok(await _service.GetAllStadiumsAsync());

        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<Stadium>> GetById([FromRoute] int id)
        {
            var stadium = await _service.GetStadiumByIdAsync(id);
            if (stadium == null) return NotFound($"Stadium with ID {id} was not found.");
            return Ok(stadium);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<Stadium>>> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword cannot be empty.");
            return Ok(await _service.SearchStadiumsAsync(keyword));
        }

        [HttpGet("paged")]
        public async Task<ActionResult<List<Stadium>>> GetPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 5)
        {
            if (pageIndex < 1 || pageSize < 1)
                return BadRequest("Page parameters must be greater than 0.");
            return Ok(await _service.GetPagedStadiumsAsync(pageIndex, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] StadiumDto dto)
        {
            if (!await _service.FeaturesExistAsync(dto.FeatureIds))
                return BadRequest("One or more selected features do not exist.");
            // تحويل البيانات من StadiumDto إلى Stadium Model
            var stadium = new Stadium
            {
                Name = dto.Name,
                Location = dto.Location,
                PricePerHour = dto.PricePerHour,
                IsAvailable = dto.IsAvailable,
                CategoryId = dto.CategoryId,
                BookingPolicy = dto.BookingPolicy != null ? new StadiumBookingPolicy
                {
                    OpeningTime = dto.BookingPolicy.OpeningTime,
                    ClosingTime = dto.BookingPolicy.ClosingTime,
                    SlotDurationMinutes = dto.BookingPolicy.SlotDurationMinutes,
                    MaxAdvanceBookingDays = dto.BookingPolicy.MaxAdvanceBookingDays,
                    CancellationDeadlineHours = dto.BookingPolicy.CancellationDeadlineHours
                } : null
            };

            stadium.StadiumFeatures = (dto.FeatureIds ?? new List<int>()).Distinct()
                .Select(id => new StadiumFeature { FeatureId = id }).ToList();
            await _service.AddStadiumAsync(stadium);
            return Ok(await _service.GetStadiumByIdAsync(stadium.Id));
        }
        // PUT: api/stadiums
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StadiumDto dto)
        {
            if (!await _service.FeaturesExistAsync(dto.FeatureIds))
                return BadRequest("One or more selected features do not exist.");
            // تحويل البيانات من StadiumDto إلى Stadium Model مع تمرير الـ id
            var stadium = new Stadium
            {
                Id = id,
                Name = dto.Name,
                Location = dto.Location,
                PricePerHour = dto.PricePerHour,
                IsAvailable = dto.IsAvailable,
                CategoryId = dto.CategoryId,
                BookingPolicy = dto.BookingPolicy != null ? new StadiumBookingPolicy
                {
                    OpeningTime = dto.BookingPolicy.OpeningTime,
                    ClosingTime = dto.BookingPolicy.ClosingTime,
                    SlotDurationMinutes = dto.BookingPolicy.SlotDurationMinutes,
                    MaxAdvanceBookingDays = dto.BookingPolicy.MaxAdvanceBookingDays,
                    CancellationDeadlineHours = dto.BookingPolicy.CancellationDeadlineHours
                } : null
            };

            if (await _service.GetStadiumByIdAsync(id) == null) return NotFound();
            await _service.UpdateStadiumAsync(stadium, dto.FeatureIds);
            return Ok(stadium);
        }
        [HttpDelete("{id:int:min(1)}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var existing = await _service.GetStadiumByIdAsync(id);
            if (existing == null) return NotFound();
            await _service.DeleteStadiumAsync(id);
            return Ok(new { Message = "Stadium deleted successfully." });
        }
    }
}