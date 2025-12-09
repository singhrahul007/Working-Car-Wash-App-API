using Microsoft.AspNetCore.Mvc;
using CarWash.Api.Data;

namespace CarWash.Api.Controllers;
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase {
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db){ _db = db; }

    [HttpGet]
    public IActionResult Get(){
        var items = _db.Products.Take(50).ToList();
        return Ok(items);
    }
}
