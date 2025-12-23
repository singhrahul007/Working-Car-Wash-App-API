using CarWash.Api.Models.DTOs.AC;

namespace CarWash.Api.Services.Interfaces.AC
{
    public interface IACServiceService
    {
        Task<ServiceResponse<List<ACServiceDTOs>>> GetAllServicesAsync(ACServiceFilterDTOs? filter = null);
        Task<ServiceResponse<ACServiceDTOs>> GetServiceByIdAsync(int id);
        Task<ServiceResponse<List<ACServiceDTOs>>> GetServicesByCategoryAsync(string category);
        Task<ServiceResponse<List<ACServiceDTOs>>> GetPopularServicesAsync();
        Task<ServiceResponse<ACBookingResponseDTOs>> CreateBookingAsync(ACBookingCreateDTOs bookingDto, Guid userId);
        Task<ServiceResponse<List<ACBookingDTOs>>> GetUserBookingsAsync(Guid userId);
        Task<ServiceResponse<ACBookingDTOs>> GetBookingByIdAsync(int bookingId, Guid userId);
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId, Guid userId);
        Task<ServiceResponse<bool>> UpdateBookingStatusAsync(int bookingId, string status);
    }
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
