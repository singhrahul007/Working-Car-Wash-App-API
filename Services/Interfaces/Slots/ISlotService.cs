using CarWash.Api.Models.DTOs.Bookings;
using CarWash.Api.Models.DTOs.Slots;

namespace CarWash.Api.Services.Interfaces.Slots
{
    public interface ISlotService
    {

        Task<SlotDTOs> GetSlotByIdAsync(int id);
        Task<IEnumerable<SlotDTOs>> GetSlotsByDateAndServiceAsync(DateTime date, int serviceId);
        Task<SlotAvailabilityDTOs> GetSlotAvailabilityAsync(DateTime date, int serviceId);
        Task<IEnumerable<SlotAvailabilityDTOs>> GetWeeklyAvailabilityAsync(DateTime startDate, int serviceId);
        Task<SlotDTOs> CreateSlotAsync(CreateSlotDTOs createSlotDto);
        Task<SlotDTOs> UpdateSlotAsync(int id, CreateSlotDTOs updateSlotDto);
        Task<bool> DeleteSlotAsync(int id);
        Task<bool> CheckSlotAvailabilityAsync(int slotId, int quantity);
        Task<BookingResultsDTOs> BookSlotAsync(BookingRequestDTOs bookingRequest);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<IEnumerable<SlotDTOs>> GenerateSlotsForServiceAsync(int serviceId, DateTime startDate, DateTime endDate);
    }
}
