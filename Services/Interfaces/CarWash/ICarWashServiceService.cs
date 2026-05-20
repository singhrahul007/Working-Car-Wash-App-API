using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarWash.Api.Models.DTOs.CarWash;

namespace CarWash.Api.Services.Interfaces.CarWash
{
    public interface ICarWashServiceService
    {
        Task<CarWashServiceResponse<List<CarWashServiceDTOs>>> GetAllServicesAsync(CarWashServiceFilterDTOs? filter = null);
        Task<CarWashServiceResponse<CarWashServiceDTOs>> GetServiceByIdAsync(int id);
        Task<CarWashServiceResponse<List<CarWashServiceDTOs>>> GetServicesByCategoryAsync(string category);
        Task<CarWashServiceResponse<List<CarWashServiceDTOs>>> GetPopularServicesAsync();
        Task<CarWashServiceResponse<CarWashBookingResponseDTOs>> CreateBookingAsync(CarWashBookingCreateDTOs bookingDto, Guid userId);
        Task<CarWashServiceResponse<List<CarWashBookingDTOs>>> GetUserBookingsAsync(Guid userId);
        Task<CarWashServiceResponse<CarWashBookingDTOs>> GetBookingByIdAsync(int bookingId, Guid userId);
        Task<CarWashServiceResponse<bool>> CancelBookingAsync(int bookingId, Guid userId);
        Task<CarWashServiceResponse<bool>> UpdateBookingStatusAsync(int bookingId, string status);
    }

    public class CarWashServiceResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
