using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Models.Entities;
using CarWash.Api.Interfaces;
using CarWash.Api.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarWash.Api.Services
{
    public class OfferService : IOfferService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OfferService> _logger;

        public OfferService(AppDbContext context, ILogger<OfferService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<List<OfferDto>>> GetAllOffersAsync(string category = null, bool activeOnly = true)
        {
            try
            {
                var query = _context.Offers.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(o => o.IsActive && o.ValidUntil >= DateTime.UtcNow);
                }

                if (!string.IsNullOrEmpty(category) && category != "all")
                {
                    query = query.Where(o => o.Category == category || o.Category == "all");
                }

                var offers = await query
                    .OrderByDescending(o => o.ValidUntil)
                    .ToListAsync();

                var offerDtos = offers.Select(MapToOfferDto).ToList();
                return ServiceResult<List<OfferDto>>.SuccessResult(offerDtos, "Offers retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offers");
                return ServiceResult<List<OfferDto>>.FailureResult($"Failed to get offers: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<OfferDto>>> GetExpiringOffersAsync(int daysThreshold = 7)
        {
            try
            {
                var expiryDate = DateTime.UtcNow.AddDays(daysThreshold);

                var offers = await _context.Offers
                    .Where(o => o.IsActive &&
                                o.ValidUntil >= DateTime.UtcNow &&
                                o.ValidUntil <= expiryDate)
                    .OrderBy(o => o.ValidUntil)
                    .ToListAsync();

                var offerDtos = offers.Select(MapToOfferDto).ToList();
                return ServiceResult<List<OfferDto>>.SuccessResult(offerDtos, "Expiring offers retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring offers");
                return ServiceResult<List<OfferDto>>.FailureResult($"Failed to get expiring offers: {ex.Message}");
            }
        }

        public async Task<ServiceResult<OfferDto>> GetOfferByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return ServiceResult<OfferDto>.FailureResult("Offer code is required");
                }

                var offer = await _context.Offers
                    .FirstOrDefaultAsync(o => o.Code == code && o.IsActive);

                if (offer == null)
                {
                    return ServiceResult<OfferDto>.FailureResult("Offer not found or inactive");
                }

                var offerDto = MapToOfferDto(offer);
                return ServiceResult<OfferDto>.SuccessResult(offerDto, "Offer retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offer by code: {Code}", code);
                return ServiceResult<OfferDto>.FailureResult($"Failed to get offer: {ex.Message}");
            }
        }

        public async Task<ServiceResult<AppliedOfferDto>> ValidateAndApplyOfferAsync(string offerCode, decimal cartAmount, List<string> serviceCategories)
        {
            try
            {
                var validationResult = await ValidateOfferAsync(offerCode, cartAmount, serviceCategories);

                if (!validationResult.Success || !validationResult.Data.IsValid)
                {
                    return ServiceResult<AppliedOfferDto>.FailureResult(
                        validationResult.Data?.Message ?? "Invalid offer");
                }

                var offer = await _context.Offers
                    .FirstOrDefaultAsync(o => o.Code == offerCode);

                if (offer == null)
                {
                    return ServiceResult<AppliedOfferDto>.FailureResult("Offer not found");
                }

                var discountAmount = validationResult.Data.DiscountAmount;
                var finalAmount = cartAmount - discountAmount;

                var appliedOffer = new AppliedOfferDto
                {
                    Code = offerCode,
                    DiscountType = offer.DiscountType,
                    DiscountValue = offer.DiscountValue,
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount,
                    Message = "Offer applied successfully"
                };

                return ServiceResult<AppliedOfferDto>.SuccessResult(appliedOffer, "Offer applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying offer: {OfferCode}", offerCode);
                return ServiceResult<AppliedOfferDto>.FailureResult($"Failed to apply offer: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> CanUserUseOfferAsync(string offerCode, Guid userId)
        {
            try
            {
                var offer = await _context.Offers
                    .FirstOrDefaultAsync(o => o.Code == offerCode && o.IsActive);

                if (offer == null)
                {
                    return ServiceResult<bool>.FailureResult("Offer not found or inactive");
                }

                // Check if offer is one-time per user
                if (offer.IsOneTimePerUser)
                {
                    var hasUsedOffer = await _context.Bookings
                        .AnyAsync(b => b.UserId == userId && b.AppliedOfferCode == offerCode);

                    if (hasUsedOffer)
                    {
                        return ServiceResult<bool>.SuccessResult(false, "You have already used this offer");
                    }
                }

                // Check usage limit
                if (offer.UsedCount >= offer.UsageLimit)
                {
                    return ServiceResult<bool>.SuccessResult(false, "This offer has reached its usage limit");
                }

                return ServiceResult<bool>.SuccessResult(true, "User can use this offer");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user can use offer: {OfferCode}, UserId: {UserId}", offerCode, userId);
                return ServiceResult<bool>.FailureResult($"Failed to validate offer usage: {ex.Message}");
            }
        }

        public async Task<ServiceResult<OfferValidationResultDto>> ValidateOfferAsync(string offerCode, decimal cartAmount, List<string> serviceCategories)
        {
            try
            {
                var offer = await _context.Offers
                    .FirstOrDefaultAsync(o => o.Code == offerCode && o.IsActive);

                if (offer == null)
                {
                    return ServiceResult<OfferValidationResultDto>.FailureResult("Offer not found or inactive");
                }

                // Check if offer is expired
                if (offer.ValidUntil < DateTime.UtcNow)
                {
                    return ServiceResult<OfferValidationResultDto>.SuccessResult(
                        new OfferValidationResultDto
                        {
                            IsValid = false,
                            Message = "This offer has expired"
                        });
                }

                // Check minimum amount
                if (cartAmount < offer.MinAmount)
                {
                    return ServiceResult<OfferValidationResultDto>.SuccessResult(
                        new OfferValidationResultDto
                        {
                            IsValid = false,
                            Message = $"Minimum order amount of {offer.MinAmount:C} required"
                        });
                }

                // Check applicable services
                if (!string.IsNullOrEmpty(offer.Category) && offer.Category != "all")
                {
                    var applicableServices = JsonSerializer.Deserialize<List<string>>(offer.ApplicableServices ?? "[]");

                    if (applicableServices != null && applicableServices.Any())
                    {
                        if (serviceCategories == null || !serviceCategories.Any(c => applicableServices.Contains(c)))
                        {
                            return ServiceResult<OfferValidationResultDto>.SuccessResult(
                                new OfferValidationResultDto
                                {
                                    IsValid = false,
                                    Message = "This offer is not applicable for selected services"
                                });
                        }
                    }
                }

                // Calculate discount amount
                decimal discountAmount = 0;
                if (offer.DiscountType == "percentage")
                {
                    discountAmount = (cartAmount * offer.DiscountValue) / 100;
                }
                else if (offer.DiscountType == "fixed")
                {
                    discountAmount = offer.DiscountValue;
                }

                // Ensure discount doesn't exceed cart amount
                discountAmount = Math.Min(discountAmount, cartAmount);

                return ServiceResult<OfferValidationResultDto>.SuccessResult(
                    new OfferValidationResultDto
                    {
                        IsValid = true,
                        Message = "Offer is valid",
                        DiscountAmount = discountAmount
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating offer: {OfferCode}", offerCode);
                return ServiceResult<OfferValidationResultDto>.FailureResult($"Failed to validate offer: {ex.Message}");
            }
        }

        public async Task<ServiceResult<int>> IncrementOfferUsageAsync(string offerCode, Guid userId)
        {
            try
            {
                var offer = await _context.Offers
                    .FirstOrDefaultAsync(o => o.Code == offerCode);

                if (offer == null)
                {
                    return ServiceResult<int>.FailureResult("Offer not found");
                }

                // Increment usage count
                offer.UsedCount++;
                offer.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ServiceResult<int>.SuccessResult(offer.UsedCount, "Offer usage incremented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing offer usage: {OfferCode}, UserId: {UserId}", offerCode, userId);
                return ServiceResult<int>.FailureResult($"Failed to increment offer usage: {ex.Message}");
            }
        }

        private OfferDto MapToOfferDto(Offer offer)
        {
            try
            {
                var applicableServices = JsonSerializer.Deserialize<List<string>>(offer.ApplicableServices ?? "[\"all\"]") ?? new List<string>();
                var daysUntilExpiry = Math.Max(0, (offer.ValidUntil - DateTime.UtcNow).Days);

                return new OfferDto
                {
                    Id = offer.Id,
                    Title = offer.Title,
                    Description = offer.Description,
                    Code = offer.Code,
                    DiscountType = offer.DiscountType,
                    DiscountValue = offer.DiscountValue,
                    MinAmount = offer.MinAmount,
                    ValidUntil = offer.ValidUntil,
                    Category = offer.Category,
                    ApplicableServices = applicableServices,
                    Icon = offer.Icon,
                    ColorCode = offer.ColorCode,
                    Terms = offer.Terms,
                    IsActive = offer.IsActive,
                    DaysUntilExpiry = daysUntilExpiry
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing applicable services for offer: {OfferId}", offer.Id);
                return new OfferDto
                {
                    Id = offer.Id,
                    Title = offer.Title,
                    Description = offer.Description,
                    Code = offer.Code,
                    DiscountType = offer.DiscountType,
                    DiscountValue = offer.DiscountValue,
                    MinAmount = offer.MinAmount,
                    ValidUntil = offer.ValidUntil,
                    Category = offer.Category,
                    ApplicableServices = new List<string>(),
                    Icon = offer.Icon,
                    ColorCode = offer.ColorCode,
                    Terms = offer.Terms,
                    IsActive = offer.IsActive,
                    DaysUntilExpiry = Math.Max(0, (offer.ValidUntil - DateTime.UtcNow).Days)
                };
            }
        }
    }
}