using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using StadiumReservationApi.Services;

namespace StadiumReservationApi.Controllers;

public class FeatureDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly FeatureService _service;
    public FeaturesController(FeatureService service) { _service = service; }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Add(FeatureDto dto) => Ok(await _service.AddAsync(dto.Name));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FeatureDto dto) =>
        await _service.UpdateAsync(id, dto.Name) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        await _service.DeleteAsync(id) ? NoContent() : NotFound();
}
