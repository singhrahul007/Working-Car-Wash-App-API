using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Interfaces;
using CarWash.Api.Models.Entities;
using CarWash.Api.Services.Interfaces;
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
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            AppDbContext context,
            IPasswordService passwordService,
            ITwoFactorService twoFactorService,
            ILogger<UserService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _twoFactorService = twoFactorService;
            _logger = logger;
        }

        public async Task<ServiceResult<UserDto>> GetProfileAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return ServiceResult<UserDto>.FailureResult("User not found");

                var userDto = MapToUserDto(user);
                return ServiceResult<UserDto>.SuccessResult(userDto, "Profile retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile for user: {UserId}", userId);
                return ServiceResult<UserDto>.FailureResult($"Failed to get profile: {ex.Message}");
            }
        }

        public async Task<ServiceResult<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto updateDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return ServiceResult<UserDto>.FailureResult("User not found");

                // Check if email is being changed and if it's already in use
                if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
                {
                    var emailExists = await _context.Users
                        .AnyAsync(u => u.Email == updateDto.Email && u.Id != userId);

                    if (emailExists)
                        return ServiceResult<UserDto>.FailureResult("Email already in use");
                }

                // Update fields
                if (!string.IsNullOrEmpty(updateDto.FullName))
                    user.FullName = updateDto.FullName;

                if (!string.IsNullOrEmpty(updateDto.Email))
                    user.Email = updateDto.Email;

                if (!string.IsNullOrEmpty(updateDto.ProfilePicture))
                    user.ProfilePicture = updateDto.ProfilePicture;

                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var updatedUserDto = MapToUserDto(user);
                return ServiceResult<UserDto>.SuccessResult(updatedUserDto, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user: {UserId}", userId);
                return ServiceResult<UserDto>.FailureResult($"Failed to update profile: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                // Validate input
                if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                    return ServiceResult<bool>.FailureResult("New password and confirmation do not match");

                if (changePasswordDto.NewPassword.Length < 6)
                    return ServiceResult<bool>.FailureResult("Password must be at least 6 characters");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.FailureResult("User not found");

                // Verify current password
                if (!_passwordService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                    return ServiceResult<bool>.FailureResult("Current password is incorrect");

                // Update password
                var newHash = _passwordService.HashPassword(changePasswordDto.NewPassword, out var newSalt);
                user.PasswordHash = newHash;
                user.PasswordSalt = newSalt;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.SuccessResult(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return ServiceResult<bool>.FailureResult($"Failed to change password: {ex.Message}");
            }
        }

        public async Task<ServiceResult<TwoFactorInfoDto>> GetTwoFactorInfoAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return ServiceResult<TwoFactorInfoDto>.FailureResult("User not found");

                var backupCodes = string.IsNullOrEmpty(user.BackupCodes)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(user.BackupCodes) ?? new List<string>();

                var twoFactorInfo = new TwoFactorInfoDto
                {
                    IsEnabled = user.TwoFactorEnabled,
                    Method = "authenticator",
                    SetupDate = $"{user.UpdatedAt:yyyy-MM-dd HH:mm:ss}",
                    QrCodeUrl = user.TwoFactorEnabled ? _twoFactorService.GenerateQrCodeUrl(user.Email, user.TwoFactorSecret) : null,
                    ManualEntryKey = user.TwoFactorEnabled ? user.TwoFactorSecret : null,
                    BackupCodes = backupCodes
                };

                return ServiceResult<TwoFactorInfoDto>.SuccessResult(twoFactorInfo, "2FA info retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting 2FA info for user: {UserId}", userId);
                return ServiceResult<TwoFactorInfoDto>.FailureResult($"Failed to get 2FA info: {ex.Message}");
            }
        }

        public async Task<ServiceResult<TwoFactorInfoDto>> SetupTwoFactorAsync(Guid userId, TwoFactorSetupDto setupDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return ServiceResult<TwoFactorInfoDto>.FailureResult("User not found");

                // Verify current password if provided
                if (!string.IsNullOrEmpty(setupDto.CurrentPassword))
                {
                    if (!_passwordService.VerifyPassword(setupDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                        return ServiceResult<TwoFactorInfoDto>.FailureResult("Current password is incorrect");
                }

                if (setupDto.Enable)
                {
                    // Generate new 2FA secret and backup codes
                    var secret = _twoFactorService.GenerateSecret();
                    var backupCodes = _twoFactorService.GenerateBackupCodes();

                    user.TwoFactorEnabled = true;
                    user.TwoFactorSecret = secret;
                    user.BackupCodes = JsonSerializer.Serialize(backupCodes);
                    user.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    var twoFactorInfo = new TwoFactorInfoDto
                    {
                        IsEnabled = true,
                        Method = setupDto.Method,
                        SetupDate = $"{user.UpdatedAt:yyyy-MM-dd HH:mm:ss}",
                        QrCodeUrl = _twoFactorService.GenerateQrCodeUrl(user.Email, secret),
                        ManualEntryKey = secret,
                        BackupCodes = backupCodes
                    };

                    return ServiceResult<TwoFactorInfoDto>.SuccessResult(twoFactorInfo, "2FA setup successfully. Save your backup codes!");
                }
                else
                {
                    // Disable 2FA
                    user.TwoFactorEnabled = false;
                    user.TwoFactorSecret = null;
                    user.BackupCodes = null;
                    user.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return ServiceResult<TwoFactorInfoDto>.SuccessResult(
                        new TwoFactorInfoDto { IsEnabled = false },
                        "2FA disabled successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up 2FA for user: {UserId}", userId);
                return ServiceResult<TwoFactorInfoDto>.FailureResult($"Failed to setup 2FA: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> VerifyTwoFactorAsync(Guid userId, TwoFactorVerifyDto verifyDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                    return ServiceResult<bool>.FailureResult("2FA is not enabled");

                var isValid = _twoFactorService.VerifyCode(user.TwoFactorSecret, verifyDto.Code);
                if (!isValid)
                    return ServiceResult<bool>.FailureResult("Invalid verification code");

                return ServiceResult<bool>.SuccessResult(true, "2FA verified successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying 2FA for user: {UserId}", userId);
                return ServiceResult<bool>.FailureResult($"Failed to verify 2FA: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DisableTwoFactorAsync(Guid userId, string currentPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.FailureResult("User not found");

                if (!user.TwoFactorEnabled)
                    return ServiceResult<bool>.SuccessResult(false, "2FA is not enabled");

                // Verify password
                if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
                    return ServiceResult<bool>.FailureResult("Current password is incorrect");

                // Disable 2FA
                user.TwoFactorEnabled = false;
                user.TwoFactorSecret = null;
                user.BackupCodes = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.SuccessResult(true, "2FA disabled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA for user: {UserId}", userId);
                return ServiceResult<bool>.FailureResult($"Failed to disable 2FA: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<AddressDto>>> GetAddressesAsync(Guid userId)
        {
            try
            {
                var addresses = await _context.Addresses
                    .Where(a => a.UserId == userId && !a.IsDeleted)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var addressDtos = addresses.Select(MapToAddressDto).ToList();
                return ServiceResult<List<AddressDto>>.SuccessResult(addressDtos, "Addresses retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for user: {UserId}", userId);
                return ServiceResult<List<AddressDto>>.FailureResult($"Failed to get addresses: {ex.Message}");
            }
        }

        public async Task<ServiceResult<AddressDto>> AddAddressAsync(Guid userId, AddressCreateDto addressDto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(addressDto.FullAddress))
                    return ServiceResult<AddressDto>.FailureResult("Address is required");

                // If this is set as default, unset other defaults
                if (addressDto.IsDefault)
                {
                    var existingDefaults = await _context.Addresses
                        .Where(a => a.UserId == userId && a.IsDefault && !a.IsDeleted)
                        .ToListAsync();

                    foreach (var existingAddress in existingDefaults)
                    {
                        existingAddress.IsDefault = false;
                    }
                }

                var address = new Address
                {
                    UserId = userId,
                    FullAddress = addressDto.FullAddress.Trim(),
                    City = addressDto.City?.Trim() ?? string.Empty,
                    State = addressDto.State?.Trim() ?? string.Empty,
                    Country = addressDto.Country?.Trim() ?? "India",
                    PostalCode = addressDto.PostalCode?.Trim() ?? string.Empty,
                    Latitude = addressDto.Latitude,
                    Longitude = addressDto.Longitude,
                    IsDefault = addressDto.IsDefault,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Addresses.AddAsync(address);
                await _context.SaveChangesAsync();

                var resultDto = MapToAddressDto(address);
                return ServiceResult<AddressDto>.SuccessResult(resultDto, "Address added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address for user: {UserId}", userId);
                return ServiceResult<AddressDto>.FailureResult($"Failed to add address: {ex.Message}");
            }
        }

        public async Task<ServiceResult<AddressDto>> UpdateAddressAsync(Guid userId, int addressId, AddressCreateDto addressDto)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);

                if (address == null)
                    return ServiceResult<AddressDto>.FailureResult("Address not found");

                // If setting as default, unset other defaults
                if (addressDto.IsDefault && !address.IsDefault)
                {
                    var existingDefaults = await _context.Addresses
                        .Where(a => a.UserId == userId && a.Id != addressId && a.IsDefault && !a.IsDeleted)
                        .ToListAsync();

                    foreach (var addr in existingDefaults)
                    {
                        addr.IsDefault = false;
                    }
                }

                // Update fields
                address.FullAddress = addressDto.FullAddress.Trim();
                address.City = addressDto.City?.Trim() ?? string.Empty;
                address.State = addressDto.State?.Trim() ?? string.Empty;
                address.Country = addressDto.Country?.Trim() ?? "India";
                address.PostalCode = addressDto.PostalCode?.Trim() ?? string.Empty;
                address.Latitude = addressDto.Latitude;
                address.Longitude = addressDto.Longitude;
                address.IsDefault = addressDto.IsDefault;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var resultDto = MapToAddressDto(address);
                return ServiceResult<AddressDto>.SuccessResult(resultDto, "Address updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for user: {UserId}", addressId, userId);
                return ServiceResult<AddressDto>.FailureResult($"Failed to update address: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAddressAsync(Guid userId, int addressId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);

                if (address == null)
                    return ServiceResult<bool>.FailureResult("Address not found");

                // Soft delete
                address.IsDeleted = true;
                address.UpdatedAt = DateTime.UtcNow;

                // If this was the default address, set another as default
                if (address.IsDefault)
                {
                    var otherAddress = await _context.Addresses
                        .FirstOrDefaultAsync(a => a.UserId == userId && a.Id != addressId && !a.IsDeleted);

                    if (otherAddress != null)
                    {
                        otherAddress.IsDefault = true;
                    }
                }

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.SuccessResult(true, "Address deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for user: {UserId}", addressId, userId);
                return ServiceResult<bool>.FailureResult($"Failed to delete address: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> SetDefaultAddressAsync(Guid userId, int addressId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);

                if (address == null)
                    return ServiceResult<bool>.FailureResult("Address not found");

                if (address.IsDefault)
                    return ServiceResult<bool>.SuccessResult(true, "Address is already default");

                // Unset other defaults
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == userId && a.Id != addressId && a.IsDefault && !a.IsDeleted)
                    .ToListAsync();

                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                }

                // Set new default
                address.IsDefault = true;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.SuccessResult(true, "Default address set successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId} for user: {UserId}", addressId, userId);
                return ServiceResult<bool>.FailureResult($"Failed to set default address: {ex.Message}");
            }
        }

        // Helper methods
        private UserDto MapToUserDto(User user)
        {
            
            var roles = user.UserRoles
                        ?.Select(ur => ur.Role?.Name)
                        .Where(name => name != null)
                        .Select(name => name!)
                        .ToArray() ?? Array.Empty<string>(); 

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                ProfilePicture = user.ProfilePicture,
                TwoFactorEnabled = user.TwoFactorEnabled,
                Roles = roles
            };
        }

        private AddressDto MapToAddressDto(Address address)
        {
            return new AddressDto
            {
                Id = address.Id,
                FullAddress = address.FullAddress,
                City = address.City,
                State = address.State,
                Country = address.Country,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault
            };
        }
    }
}