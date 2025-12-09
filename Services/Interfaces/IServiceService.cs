using CarWash.Api.DTOs;
using CarWash.Api.DTOs.CarWash.Api.DTOs;
using CarWash.Api.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWash.Api.Interfaces
{
    public interface IServiceService
    {
        Task<ServiceResult<List<ServiceDto>>> GetAllServicesAsync(string category = null);
        Task<ServiceResult<ServiceDto>> GetServiceByIdAsync(int id);
        Task<ServiceResult<List<ServiceDto>>> GetPopularServicesAsync();
        Task<ServiceResult<ServiceAvailabilityDto>> CheckAvailabilityAsync(int serviceId, DateTime date);
        Task<ServiceResult<bool>> IsTimeSlotAvailableAsync(int serviceId, DateTime date, string timeSlot);
    }
}