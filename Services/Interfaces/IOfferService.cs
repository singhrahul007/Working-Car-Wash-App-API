using CarWash.Api.DTOs;
using CarWash.Api.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWash.Api.Interfaces
{
    public interface IOfferService
    {
        Task<ServiceResult<List<OfferDto>>> GetAllOffersAsync(string category = null, bool activeOnly = true);
        Task<ServiceResult<List<OfferDto>>> GetExpiringOffersAsync(int daysThreshold = 7);
        Task<ServiceResult<OfferDto>> GetOfferByCodeAsync(string code);
        Task<ServiceResult<AppliedOfferDto>> ValidateAndApplyOfferAsync(string offerCode, decimal cartAmount, List<string> serviceCategories);
        Task<ServiceResult<bool>> CanUserUseOfferAsync(string offerCode, Guid userId);
        Task<ServiceResult<OfferValidationResultDto>> ValidateOfferAsync(string offerCode, decimal cartAmount, List<string> serviceCategories);
        Task<ServiceResult<int>> IncrementOfferUsageAsync(string offerCode, Guid userId);
    }
}