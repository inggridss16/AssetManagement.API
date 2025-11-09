// Services/MaintenanceService.cs
using AssetManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Added for logging
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagement.API.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AssetManagementDbContext _context;
        private readonly ILogger<MaintenanceService> _logger; // Added for logging

        public MaintenanceService(AssetManagementDbContext context, ILogger<MaintenanceService> logger)
        {
            _context = context;
            _logger = logger; // Injected logger
        }

        /// <summary>
        /// Adds a new maintenance record and updates the total asset value.
        /// </summary>
        public async Task<TrxMaintenanceRecord> AddMaintenanceRecordAsync(TrxMaintenanceRecord record, long currentUserId)
        {
            try
            {
                record.CreatedBy = currentUserId;
                record.CreatedDate = DateTime.UtcNow;

                _context.TrxMaintenanceRecords.Add(record);
                await _context.SaveChangesAsync();
                await UpdateTotalAssetValue(record.LinkedAssetId);
                return record;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding maintenance record for asset {AssetId}", record.LinkedAssetId);
                throw; // Re-throwing the exception to be handled by a global exception handler
            }
        }

        /// <summary>
        /// Updates an existing maintenance record and recalculates the total asset value.
        /// </summary>
        public async Task<TrxMaintenanceRecord> UpdateMaintenanceRecordAsync(TrxMaintenanceRecord record, long currentUserId)
        {
            try
            {
                var existingRecord = await _context.TrxMaintenanceRecords.FindAsync(record.Id);
                if (existingRecord == null)
                {
                    // Or throw a specific NotFoundException
                    return null;
                }

                existingRecord.UpdatedBy = currentUserId;
                existingRecord.UpdatedDate = DateTime.UtcNow;
                existingRecord.MaintenanceCost = record.MaintenanceCost;
                // Update other properties as needed...


                _context.TrxMaintenanceRecords.Update(existingRecord);
                await _context.SaveChangesAsync();
                await UpdateTotalAssetValue(existingRecord.LinkedAssetId);
                return existingRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating maintenance record with ID {RecordId}", record.Id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a maintenance record and recalculates the total asset value.
        /// </summary>
        public async Task<bool> DeleteMaintenanceRecordAsync(long id)
        {
            try
            {
                var record = await _context.TrxMaintenanceRecords.FindAsync(id);
                if (record == null)
                {
                    _logger.LogWarning("Delete operation failed: Maintenance record with ID {RecordId} not found.", id);
                    return false;
                }

                var linkedAssetId = record.LinkedAssetId;
                _context.TrxMaintenanceRecords.Remove(record);
                await _context.SaveChangesAsync();
                await UpdateTotalAssetValue(linkedAssetId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting maintenance record with ID {RecordId}", id);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single maintenance record by its ID.
        /// </summary>
        public async Task<TrxMaintenanceRecord> GetMaintenanceRecordByIdAsync(long id)
        {
            try
            {
                return await _context.TrxMaintenanceRecords.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving maintenance record with ID {RecordId}", id);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all maintenance records for a specific asset.
        /// </summary>
        public async Task<IEnumerable<TrxMaintenanceRecord>> GetMaintenanceRecordsForAssetAsync(string assetId)
        {
            try
            {
                return await _context.TrxMaintenanceRecords
                    .Where(r => r.LinkedAssetId == assetId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving maintenance records for asset {AssetId}", assetId);
                throw;
            }
        }

        /// <summary>
        /// Requirement 14: Updates the AssetValue on the parent asset.
        /// </summary>
        private async Task UpdateTotalAssetValue(string assetId)
        {
            try
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
                else
                {
                    _logger.LogWarning("Could not update total asset value. Asset with ID {AssetId} not found.", assetId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating total asset value for asset {AssetId}", assetId);
                throw;
            }
        }
    }
}