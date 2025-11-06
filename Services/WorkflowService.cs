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

        public async Task StartApprovalProcessAsync(string assetId, long requesterId)
        {
            var requester = await _context.MstUsers.FindAsync(requesterId);
            if (requester == null)
            {
                throw new System.Exception("Requester not found.");
            }

            var approvals = new List<TrxAssetApproval>();

            // MODIFIED: Add approval for the Department Head (Supervisor) only if SupervisorId is not 0
            if (requester.SupervisorId != 0)
            {
                approvals.Add(new TrxAssetApproval
                {
                    AssetId = assetId,
                    ApproverId = requester.SupervisorId,
                    Status = "Pending",
                    CreatedBy = requesterId,
                    CreatedDate = DateTime.UtcNow,
                });
            }

            // Find all Asset Managers in the same department
            var assetManagers = await _context.MstUsers
                .Where(u => u.DepartmentId == requester.DepartmentId && u.IsAssetManager)
                .ToListAsync();

            if (!assetManagers.Any())
            {
                throw new System.Exception($"No Asset Manager found in department ID {requester.DepartmentId}.");
            }

            foreach (var manager in assetManagers)
            {
                approvals.Add(new TrxAssetApproval
                {
                    AssetId = assetId,
                    ApproverId = manager.Id,
                    Status = "Pending",
                    CreatedBy = requesterId,
                    CreatedDate = DateTime.UtcNow,
                });
            }

            _context.TrxAssetApprovals.AddRange(approvals);
            await _context.SaveChangesAsync();
        }

        public async Task SubmitApprovalAsync(string assetId, long approverId, bool isApproved, string comments)
        {
            try
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
                await CheckWorkflowStatusAsync(assetId);
            }
            catch (DbUpdateException ex)
            {
                // This will catch errors related to database updates.
                Console.WriteLine($"An error occurred while updating the database: {ex.Message}");
                // You can also log the inner exception for more details.
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                // Depending on your application's needs, you might want to re-throw the exception
                // or handle it in a way that makes sense for your workflow.
                throw;
            }
            catch (Exception ex)
            {
                // This is a general catch block for any other exceptions.
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                // It's often a good practice to re-throw the exception after logging it,
                // so the calling code is aware that an error occurred.
                throw;
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

            // MODIFIED: Determine if department head approval is required and check its status.
            bool isHeadApproved = requester.SupervisorId == 0; // Default to true if no supervisor
            if (!isHeadApproved)
            {
                isHeadApproved = approvals.Any(a => a.ApproverId == requester.SupervisorId && a.Status == "Approved");
            }

            // Check if at least one asset manager has approved
            var assetManagerIds = await _context.MstUsers
                .Where(u => u.DepartmentId == requester.DepartmentId && u.IsAssetManager)
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