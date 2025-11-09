using AssetManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AssetManagement.API.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly AssetManagementDbContext _context;

        public WorkflowService(AssetManagementDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<TrxAsset>> GetPendingApprovalsAsync(long managerId)
        {
            var pendingAssetIds = await _context.TrxAssetApprovals
                .Where(a => a.ApproverId == managerId && a.Status == "Pending")
                .Select(a => a.AssetId)
                .ToListAsync();

            return await _context.TrxAssets
                .Where(a => pendingAssetIds.Contains(a.Id)).ToListAsync();
        }

        /// <summary>
        /// Starts the first step of the approval process.
        /// An approval is created for the requester's supervisor.
        /// If no supervisor is assigned, it moves directly to the asset managers.
        /// </summary>
        public async Task StartApprovalProcessAsync(string assetId, long requesterId)
        {
            var requester = await _context.MstUsers.FindAsync(requesterId);
            if (requester == null)
            {
                throw new System.Exception("Requester not found.");
            }

            // If a supervisor exists (and is not the user themselves), create their approval record.
            if (requester.SupervisorId != 0 && requester.SupervisorId != requesterId)
            {
                var supervisorApproval = new TrxAssetApproval
                {
                    AssetId = assetId,
                    ApproverId = requester.SupervisorId,
                    Status = "Pending",
                    CreatedBy = requesterId,
                    CreatedDate = DateTime.UtcNow,
                };
                _context.TrxAssetApprovals.Add(supervisorApproval);
                await _context.SaveChangesAsync();
            }
            else
            {
                // No supervisor, so go directly to Asset Manager approval.
                await CreateAssetManagerApprovalsAsync(assetId, requesterId);
            }
        }

        /// <summary>
        /// Submits an approval or rejection for an asset and advances the workflow.
        /// </summary>
        public async Task SubmitApprovalAsync(string assetId, long approverId, bool isApproved, string comments)
        {
            var approval = await _context.TrxAssetApprovals
                .FirstOrDefaultAsync(a => a.AssetId == assetId && a.ApproverId == approverId);

            if (approval == null || approval.Status != "Pending")
            {
                throw new System.Exception("No pending approval found for this user and asset.");
            }

            approval.Status = isApproved ? "Approved" : "Rejected";
            approval.Comments = comments;
            approval.ApprovalDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // After a successful approval, check if we need to move to the next stage.
            if (isApproved)
            {
                var asset = await _context.TrxAssets.FindAsync(assetId);
                if (asset == null)
                {
                    throw new System.Exception("Asset not found after approval submission.");
                }

                var requester = await _context.MstUsers.FindAsync(asset.RequesterId);
                if (requester == null)
                {
                    throw new System.Exception("Requester not found.");
                }

                // If the approver was the supervisor, trigger the next stage for Asset Managers.
                if (approverId == requester.SupervisorId)
                {
                    await CreateAssetManagerApprovalsAsync(assetId, asset.RequesterId);
                }
            }

            // Check if the workflow is now complete or should be discarded.
            await CheckWorkflowStatusAsync(assetId);
        }

        /// <summary>
        /// Creates "Pending" approval records for all asset managers.
        /// </summary>
        private async Task CreateAssetManagerApprovalsAsync(string assetId, long requesterId)
        {
            var assetManagers = await _context.MstUsers
                .Where(u => u.IsAssetManager)
                .ToListAsync();

            if (!assetManagers.Any())
            {
                throw new System.Exception("No Asset Manager found in the system to process the approval.");
            }

            var managerApprovals = new List<TrxAssetApproval>();
            foreach (var manager in assetManagers)
            {
                // Only add if an approval for this manager on this asset doesn't already exist.
                bool approvalExists = await _context.TrxAssetApprovals.AnyAsync(a => a.AssetId == assetId && a.ApproverId == manager.Id);
                if (!approvalExists)
                {
                    managerApprovals.Add(new TrxAssetApproval
                    {
                        AssetId = assetId,
                        ApproverId = manager.Id,
                        Status = "Pending",
                        CreatedBy = requesterId,
                        CreatedDate = DateTime.UtcNow,
                    });
                }
            }

            if (managerApprovals.Any())
            {
                _context.TrxAssetApprovals.AddRange(managerApprovals);
                await _context.SaveChangesAsync();
            }
        }


        private async Task CheckWorkflowStatusAsync(string assetId)
        {
            var asset = await _context.TrxAssets.FindAsync(assetId);
            if (asset == null) return;

            var requester = await _context.MstUsers.FindAsync(asset.RequesterId);
            if (requester == null) return;

            var approvals = await _context.TrxAssetApprovals
                .Where(a => a.AssetId == assetId)
                .ToListAsync();

            if (approvals.Any(a => a.Status == "Rejected"))
            {
                asset.Status = "Discarded";
                await _context.SaveChangesAsync();
                return;
            }

            // Determine if department head approval is required and check its status.
            bool isHeadApproved = requester.SupervisorId == 0 || requester.SupervisorId == requester.Id; // Default to true if no supervisor or is self-supervised
            if (!isHeadApproved)
            {
                isHeadApproved = approvals.Any(a => a.ApproverId == requester.SupervisorId && a.Status == "Approved");
            }

            // Check if at least one asset manager has approved
            var assetManagerIds = await _context.MstUsers
                .Where(u => u.IsAssetManager)
                .Select(u => u.Id)
                .ToListAsync();

            bool isManagerApproved = approvals.Any(a => assetManagerIds.Contains(a.ApproverId) && a.Status == "Approved");

            // If all required approvals are met, the asset is assigned.
            if (isHeadApproved && isManagerApproved)
            {
                asset.Status = "Assigned";
            }

            await _context.SaveChangesAsync();
        }
    }
}