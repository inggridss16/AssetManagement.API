// Services/MaintenanceService.cs
using AssetManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagement.API.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AssetManagementDbContext _context;

        public MaintenanceService(AssetManagementDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new maintenance record and updates the total asset value.
        /// </summary>
        public async Task<TrxMaintenanceRecord> AddMaintenanceRecordAsync(TrxMaintenanceRecord record, long currentUserId)
        {
            record.CreatedBy = currentUserId;
            record.CreatedDate = DateTime.UtcNow;

            _context.TrxMaintenanceRecords.Add(record);
            await _context.SaveChangesAsync();
            await UpdateTotalAssetValue(record.LinkedAssetId);
            return record;
        }

        /// <summary>
        /// Updates an existing maintenance record and recalculates the total asset value.
        /// </summary>
        public async Task<TrxMaintenanceRecord> UpdateMaintenanceRecordAsync(TrxMaintenanceRecord record, long currentUserId)
        {
            record.UpdatedBy = currentUserId;
            record.UpdatedDate = DateTime.UtcNow;

            _context.TrxMaintenanceRecords.Update(record);
            await _context.SaveChangesAsync();
            await UpdateTotalAssetValue(record.LinkedAssetId);
            return record;
        }

        /// <summary>
        /// Deletes a maintenance record and recalculates the total asset value.
        /// </summary>
        public async Task<bool> DeleteMaintenanceRecordAsync(long id)
        {
            var record = await _context.TrxMaintenanceRecords.FindAsync(id);
            if (record == null)
            {
                return false;
            }

            var linkedAssetId = record.LinkedAssetId;
            _context.TrxMaintenanceRecords.Remove(record);
            await _context.SaveChangesAsync();
            await UpdateTotalAssetValue(linkedAssetId);
            return true;
        }

        /// <summary>
        /// Retrieves a single maintenance record by its ID.
        /// </summary>
        public async Task<TrxMaintenanceRecord> GetMaintenanceRecordByIdAsync(long id)
        {
            return await _context.TrxMaintenanceRecords.FindAsync(id);
        }

        /// <summary>
        /// Retrieves all maintenance records for a specific asset.
        /// </summary>
        public async Task<IEnumerable<TrxMaintenanceRecord>> GetMaintenanceRecordsForAssetAsync(string assetId)
        {
            return await _context.TrxMaintenanceRecords
                .Where(r => r.LinkedAssetId == assetId)
                .ToListAsync();
        }

        /// <summary>
        /// Requirement 14: Updates the AssetValue on the parent asset.
        /// </summary>
        private async Task UpdateTotalAssetValue(string assetId)
        {
            var asset = await _context.TrxAssets.FindAsync(assetId);
            if (asset != null)
            {
                asset.AssetValue = await _context.TrxMaintenanceRecords
                    .Where(r => r.LinkedAssetId == assetId)
                    .SumAsync(r => r.MaintenanceCost);

                // Requirement 15: Encryption for AssetValue should be handled here
                // before saving. For example, using a data protection provider.

                await _context.SaveChangesAsync();
            }
        }
    }
}