// Services/Interfaces/ICacheService.cs
using System;
using System.Threading.Tasks;

namespace CarWash.Api.Services.Interfaces
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}