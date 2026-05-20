using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarWash.Api.Models.DTOs.BikeWash;

namespace CarWash.Api.Services.Interfaces.BikeWash
{
    public interface IBikeWashServiceService
    {
        Task<BikeWashServiceResponse<List<BikeWashServiceDTOs>>> GetAllServicesAsync(BikeWashServiceFilterDTOs? filter = null);
        Task<BikeWashServiceResponse<BikeWashServiceDTOs>> GetServiceByIdAsync(int id);
        Task<BikeWashServiceResponse<List<BikeWashServiceDTOs>>> GetServicesByCategoryAsync(string category);
        Task<BikeWashServiceResponse<List<BikeWashServiceDTOs>>> GetPopularServicesAsync();
        Task<BikeWashServiceResponse<BikeWashBookingResponseDTOs>> CreateBookingAsync(BikeWashBookingCreateDTOs bookingDto, Guid userId);
        Task<BikeWashServiceResponse<List<BikeWashBookingDTOs>>> GetUserBookingsAsync(Guid userId);
        Task<BikeWashServiceResponse<BikeWashBookingDTOs>> GetBookingByIdAsync(int bookingId, Guid userId);
        Task<BikeWashServiceResponse<bool>> CancelBookingAsync(int bookingId, Guid userId);
        Task<BikeWashServiceResponse<bool>> UpdateBookingStatusAsync(int bookingId, string status);
    }

    public class BikeWashServiceResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
