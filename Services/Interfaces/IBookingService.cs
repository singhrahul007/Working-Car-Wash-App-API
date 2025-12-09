// Interfaces/IBookingService.cs
using CarWash.Api.DTOs;
using CarWash.Api.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWash.Api.Interfaces
{
    public interface IBookingService
    {
        Task<ServiceResult<BookingDto>> CreateBookingAsync(Guid userId, BookingCreateDto bookingDto);
        Task<ServiceResult<List<BookingDto>>> GetUserBookingsAsync(Guid userId, string status = null);
        Task<ServiceResult<BookingDto>> GetBookingByIdAsync(string bookingId, Guid userId);
        Task<ServiceResult<bool>> CancelBookingAsync(string bookingId, Guid userId);
        Task<ServiceResult<CartDto>> GetCartAsync(Guid userId);
        Task<ServiceResult<CartDto>> AddToCartAsync(Guid userId, CartItemDto cartItem);
        Task<ServiceResult<CartDto>> RemoveFromCartAsync(Guid userId, int serviceId);
        Task<ServiceResult<CartDto>> UpdateCartItemAsync(Guid userId, CartItemDto cartItem);
        Task<ServiceResult<CartDto>> ApplyOfferToCartAsync(Guid userId, string offerCode);
        Task<ServiceResult<CartDto>> RemoveOfferFromCartAsync(Guid userId);
        Task<ServiceResult<decimal>> CalculateCartTotalAsync(Guid userId);
    }
}