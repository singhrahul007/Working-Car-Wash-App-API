using CarWash.Api.Models.DTOs.CarWash;
using CarWash.Api.Services.Interfaces.CarWash;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/car-services")]
    public class CarWashServicesController : ControllerBase
    {
        private readonly ICarWashServiceService _carWashService;

        public CarWashServicesController(ICarWashServiceService carWashService)
        {
            _carWashService = carWashService;
        }

        /// <summary>GET /api/car-services — list all active car services (optional ?Category=&amp;IsPopular=)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CarWashServiceFilterDTOs filter)
        {
            var result = await _carWashService.GetAllServicesAsync(filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>GET /api/car-services/popular</summary>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            var result = await _carWashService.GetPopularServicesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>GET /api/car-services/category/{category}</summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var result = await _carWashService.GetServicesByCategoryAsync(category);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>GET /api/car-services/{id}</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _carWashService.GetServiceByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
