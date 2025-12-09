using CarWash.Api.DTOs;
using CarWash.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string category = null)
        {
            var result = await _offerService.GetAllOffersAsync(category);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("expiring-soon")]
        public async Task<IActionResult> GetExpiringSoon()
        {
            var result = await _offerService.GetExpiringOffersAsync();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var result = await _offerService.GetOfferByCodeAsync(code);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateOffer([FromBody] ApplyOfferDto applyOfferDto)
        {
            var result = await _offerService.ValidateAndApplyOfferAsync(
                applyOfferDto.OfferCode,
                applyOfferDto.CartAmount,
                applyOfferDto.ServiceCategories);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}