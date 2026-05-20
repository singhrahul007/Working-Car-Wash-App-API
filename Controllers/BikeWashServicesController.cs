using CarWash.Api.Models.DTOs.BikeWash;
using CarWash.Api.Services.Interfaces.BikeWash;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/bike-services")]
    public class BikeWashServicesController : ControllerBase
    {
        private readonly IBikeWashServiceService _bikeWashService;

        public BikeWashServicesController(IBikeWashServiceService bikeWashService)
        {
            _bikeWashService = bikeWashService;
        }

        /// <summary>GET /api/bike-services — list all active bike services (optional ?Category=&amp;IsPopular=)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BikeWashServiceFilterDTOs filter)
        {
            var result = await _bikeWashService.GetAllServicesAsync(filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>GET /api/bike-services/popular</summary>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            var result = await _bikeWashService.GetPopularServicesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>GET /api/bike-services/category/{category}</summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var result = await _bikeWashService.GetServicesByCategoryAsync(category);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>GET /api/bike-services/{id}</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _bikeWashService.GetServiceByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
