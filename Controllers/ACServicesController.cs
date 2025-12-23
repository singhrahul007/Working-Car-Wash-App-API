// Controllers/ACServicesController.cs
using CarWash.Api.Models.DTOs;
using CarWash.Api.Models.DTOs.AC;
using CarWash.Api.Services.Interfaces;
using CarWash.Api.Services.Interfaces.AC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/ac-services")]
    public class ACServicesController : ControllerBase
    {
        private readonly IACServiceService _acServiceService;

        public ACServicesController(IACServiceService acServiceService)
        {
            _acServiceService = acServiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ACServiceFilterDTOs? filter)
        {
            var result = await _acServiceService.GetAllServicesAsync(filter);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var result = await _acServiceService.GetServicesByCategoryAsync(category);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular()
        {
            var result = await _acServiceService.GetPopularServicesAsync();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _acServiceService.GetServiceByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}