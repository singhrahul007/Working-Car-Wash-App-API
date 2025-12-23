// Services/Interfaces/IUserService.cs
using CarWash.Api.DTOs;
using CarWash.Api.Models.Entities;
using CarWash.Api.Utilities;


namespace CarWash.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<UserDto>> GetProfileAsync(Guid userId);
        Task<ServiceResult<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto updateDto);
        Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);

        Task<ServiceResult<TwoFactorInfoDto>> GetTwoFactorInfoAsync(Guid userId);
        Task<ServiceResult<TwoFactorInfoDto>> SetupTwoFactorAsync(Guid userId, TwoFactorSetupDto setupDto);
        Task<ServiceResult<bool>> VerifyTwoFactorAsync(Guid userId, TwoFactorVerifyDto verifyDto);
        Task<ServiceResult<bool>> DisableTwoFactorAsync(Guid userId, string currentPassword);

        Task<ServiceResult<List<AddressDto>>> GetAddressesAsync(Guid userId);
        Task<ServiceResult<AddressDto>> AddAddressAsync(Guid userId, AddressCreateDto addressDto);
        Task<ServiceResult<AddressDto>> UpdateAddressAsync(Guid userId, int addressId, AddressCreateDto addressDto);
        Task<ServiceResult<bool>> DeleteAddressAsync(Guid userId, int addressId);
        Task<ServiceResult<bool>> SetDefaultAddressAsync(Guid userId, int addressId);
    }
}