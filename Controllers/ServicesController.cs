using CarWash.Api.Data;
using CarWash.Api.Interfaces;
using CarWash.Api.Models;
using CarWash.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers;
[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase {
    private readonly AppDbContext _db;
    private readonly IServiceService _serviceService;
    public ServicesController(AppDbContext db, IServiceService serviceService)
    {
        _db = db;
        _serviceService = serviceService;
    }

    [HttpGet]
    public IActionResult Get(){
        var items = _db.Services.Take(20).ToList();
        return Ok(items);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string category = null)
    {
        var result = await _serviceService.GetAllServicesAsync(category);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopular()
    {
        var result = await _serviceService.GetPopularServicesAsync();

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _serviceService.GetServiceByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
    [HttpGet("{id}/availability")]
    public async Task<IActionResult> CheckAvailability(int id, [FromQuery] DateTime date)
    {
        var result = await _serviceService.CheckAvailabilityAsync(id, date);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("{id}/check-slot")]
    public async Task<IActionResult> CheckTimeSlot(int id, [FromQuery] DateTime date, [FromQuery] string timeSlot)
    {
        var result = await _serviceService.IsTimeSlotAvailableAsync(id, date, timeSlot);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
