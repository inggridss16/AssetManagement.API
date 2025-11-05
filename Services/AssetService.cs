using AssetManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagement.API.Services
{
    public class AssetService : IAssetService
    {
        private readonly AssetManagementDbContext _context;
        private readonly IWorkflowService _workflowService;

        public AssetService(AssetManagementDbContext context, IWorkflowService workflowService)
        {
            _context = context;
            _workflowService = workflowService;

        }

        /// <summary>
        /// Creates a new asset.
        /// </summary>
        public async Task<TrxAsset> CreateAssetAsync(TrxAsset asset, long currentUserId)
        {
            // Requirement 3: Default Values
            asset.Status = "New";
            asset.RequesterId = currentUserId;

            // Requirement 2.1: Autonumbering for Asset ID
            var lastAsset = await _context.TrxAssets.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            var lastId = (lastAsset == null) ? 0 : int.Parse(lastAsset.Id.Substring(4));
            asset.Id = $"AST-{(lastId + 1):D3}";

            // Requirement 5.3: Description is mandatory for a "New" asset
            if (asset.Status == "New" && string.IsNullOrWhiteSpace(asset.Description))
            {
                throw new InvalidOperationException("Description is mandatory for new assets.");
            }

            asset.CreatedBy = currentUserId;
            asset.CreatedDate = DateTime.UtcNow;

            _context.TrxAssets.Add(asset);
            await _context.SaveChangesAsync();
            return asset;
        }

        /// <summary>
        /// Retrieves all assets.
        /// </summary>
        public async Task<IEnumerable<TrxAsset>> GetAllAssetsAsync()
        {
            return await _context.TrxAssets.ToListAsync();
        }

        /// <summary>
        /// Retrieves a single asset by its ID.
        /// </summary>
        public async Task<TrxAsset> GetAssetByIdAsync(string id)
        {
            return await _context.TrxAssets.FindAsync(id);
        }

        /// <summary>
        /// Updates an existing asset.
        /// </summary>
        public async Task<TrxAsset> UpdateAssetAsync(TrxAsset asset, long currentUserId)
        {
            var existingAsset = await _context.TrxAssets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == asset.Id);

            if (existingAsset == null)
            {
                return null;
            }

            // Requirement 5.2: If original status was "Assigned", Requester field is read-only
            if (existingAsset.Status == "Assigned")
            {
                asset.RequesterId = existingAsset.RequesterId;
            }

            // Requirement 13: Approval Workflow Logic
            if (asset.Status == "Under Review" && existingAsset.Status != "Under Review")
            {
                await _workflowService.StartApprovalProcessAsync(asset.Id, asset.RequesterId);
            }


            // Set audit fields for the update
            asset.UpdatedBy = currentUserId;
            asset.UpdatedDate = DateTime.UtcNow;

            _context.Entry(asset).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return asset;
        }

        /// <summary>
        /// Deletes an asset.
        /// </summary>
        public async Task<bool> DeleteAssetAsync(string id)
        {
            // Note: Permission checks (e.g., IT Department users cannot delete) should be handled in the controller.
            var asset = await _context.TrxAssets.FindAsync(id);
            if (asset == null)
            {
                return false;
            }

            _context.TrxAssets.Remove(asset);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}