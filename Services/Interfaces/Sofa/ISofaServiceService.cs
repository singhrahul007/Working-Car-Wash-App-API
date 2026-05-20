using CarWash.Api.Models.DTOs.Sofa;

namespace CarWash.Api.Services.Interfaces.Sofa
{
    public interface ISofaServiceService
    {
        Task<SofaServiceResponse<List<SofaServiceDTOs>>> GetAllServicesAsync(SofaServiceFilterDTOs? filter = null);
        Task<SofaServiceResponse<SofaServiceDTOs>> GetServiceByIdAsync(int id);
        Task<SofaServiceResponse<List<SofaServiceDTOs>>> GetServicesByCategoryAsync(string category);
        Task<SofaServiceResponse<List<SofaServiceDTOs>>> GetPopularServicesAsync();
        Task<SofaServiceResponse<SofaBookingResponseDTOs>> CreateBookingAsync(SofaBookingCreateDTOs bookingDto, Guid userId);
        Task<SofaServiceResponse<List<SofaBookingDTOs>>> GetUserBookingsAsync(Guid userId);
        Task<SofaServiceResponse<SofaBookingDTOs>> GetBookingByIdAsync(int bookingId, Guid userId);
        Task<SofaServiceResponse<bool>> CancelBookingAsync(int bookingId, Guid userId);
        Task<SofaServiceResponse<bool>> UpdateBookingStatusAsync(int bookingId, string status);
    }

    public class SofaServiceResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
