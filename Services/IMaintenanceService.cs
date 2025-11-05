// Services/IMaintenanceService.cs
using AssetManagement.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetManagement.API.Services
{
    /// <summary>
    /// Service for managing maintenance records.
    /// </summary>
    public interface IMaintenanceService
    {
        Task<IEnumerable<TrxMaintenanceRecord>> GetMaintenanceRecordsForAssetAsync(string assetId);
        Task<TrxMaintenanceRecord> GetMaintenanceRecordByIdAsync(long id);
        Task<TrxMaintenanceRecord> AddMaintenanceRecordAsync(TrxMaintenanceRecord record, long currentUserId);
        Task<TrxMaintenanceRecord> UpdateMaintenanceRecordAsync(TrxMaintenanceRecord record, long currentUserId);
        Task<bool> DeleteMaintenanceRecordAsync(long id);
    }
}