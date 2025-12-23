using CarWash.Api.Data;
using CarWash.Api.Models.Entities;
using CarWash.Api.Interfaces;
using CarWash.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers;
[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase {
    private readonly AppDbContext _db;
    private readonly IBookingService _bookingService;
    public BookingsController(AppDbContext db, IBookingService bookingService) { _db = db; _bookingService = bookingService; }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public IActionResult Create([FromBody] Booking booking){
        booking.BookingId = GenerateBookingId();
        booking.CreatedAt = DateTime.UtcNow;
        booking.Status = "pending";
        // set user id from claims if missing
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if(!string.IsNullOrEmpty(userIdClaim)){
            booking.UserId = Guid.Parse(userIdClaim);
        }
        _db.Bookings.Add(booking);
        _db.SaveChanges();
        return Ok(new { bookingId = booking.BookingId, status = booking.Status });
    }

    [HttpGet("recent")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public IActionResult Recent(){
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if(string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        var uid = Guid.Parse(userIdClaim);
        var bookings = _db.Bookings.Where(b => b.UserId == uid).OrderByDescending(b => b.CreatedAt).Take(10).ToList();
        return Ok(bookings);
    }
    private string GenerateBookingId()
    {
        var date = DateTime.UtcNow.ToString("yyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"CW{date}{random}";
    }
}
