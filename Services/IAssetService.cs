// Services/IAssetService.cs
using AssetManagement.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetManagement.API.Services
{
    /// <summary>
    /// Service for managing assets.
    /// </summary>
    public interface IAssetService
    {
        Task<IEnumerable<TrxAsset>> GetAllAssetsAsync();
        Task<TrxAsset> GetAssetByIdAsync(string id);
        Task<TrxAsset> CreateAssetAsync(TrxAsset asset, long currentUserId);
        Task<TrxAsset> UpdateAssetAsync(TrxAsset asset, long currentUserId);
        Task<bool> DeleteAssetAsync(string id);
    }
}