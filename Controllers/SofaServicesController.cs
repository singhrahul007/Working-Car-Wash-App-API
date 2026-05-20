// Controllers/SofaServicesController.cs
using CarWash.Api.Models.DTOs.Sofa;
using CarWash.Api.Services.Interfaces.Sofa;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/sofa-services")]
    public class SofaServicesController : ControllerBase
    {
        private readonly ISofaServiceService _sofaService;

        public SofaServicesController(ISofaServiceService sofaService)
        {
            _sofaService = sofaService;
        }

        /// <summary>GET /api/sofa-services — list all active sofa services (optional ?Category=&amp;IsPopular=)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SofaServiceFilterDTOs filter)
        {
            var result = await _sofaService.GetAllServicesAsync(filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>GET /api/sofa-services/popular</summary>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            var result = await _sofaService.GetPopularServicesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>GET /api/sofa-services/category/{category}</summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var result = await _sofaService.GetServicesByCategoryAsync(category);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>GET /api/sofa-services/{id}</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sofaService.GetServiceByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
