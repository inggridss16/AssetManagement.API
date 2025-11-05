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

            // Add approval for the Department Head (Supervisor)
            var departmentHeadId = requester.SupervisorId;
            approvals.Add(new TrxAssetApproval
            {
                AssetId = assetId,
                ApproverId = departmentHeadId,
                Status = "Pending",
                CreatedBy = requesterId,
                CreatedDate = DateTime.UtcNow,
            });

            // Find all Asset Managers in the same department
            var assetManagers = await _context.MstUsers
                .Where(u => u.DepartmentId == requester.DepartmentId && u.IsAssetManager)
                .ToListAsync();

            if (!assetManagers.Any())
            {
                // Handle case where no asset manager is found in the department
                // Depending on business rules, this could throw an exception or auto-approve this step.
                // For now, let's throw an exception.
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
            if (asset == null) return; // Asset not found, exit

            var requester = await _context.MstUsers.FindAsync(asset.RequesterId);
            if (requester == null) return; // Requester not found, exit

            var approvals = await _context.TrxAssetApprovals
                .Where(a => a.AssetId == assetId)
                .ToListAsync();

            // If any approver rejects, the process ends, and the asset is discarded.
            if (approvals.Any(a => a.Status == "Rejected"))
            {
                asset.Status = "Discarded";
                await _context.SaveChangesAsync();
                return;
            }

            // Identify the required approvers
            var departmentHeadId = requester.SupervisorId;
            var assetManagerIds = await _context.MstUsers
                .Where(u => u.DepartmentId == requester.DepartmentId && u.IsAssetManager)
                .Select(u => u.Id)
                .ToListAsync();

            // Check if the department head has approved
            bool isHeadApproved = approvals.Any(a => a.ApproverId == departmentHeadId && a.Status == "Approved");

            // Check if at least one asset manager has approved
            bool isManagerApproved = approvals.Any(a => assetManagerIds.Contains(a.ApproverId) && a.Status == "Approved");

            // If both conditions are met, the asset is assigned.
            if (isHeadApproved && isManagerApproved)
            {
                asset.Status = "Assigned";
            }
            // Note: If conditions are not met, but there are no rejections,
            // the asset status remains "Under Review" until all required approvals are in.

            await _context.SaveChangesAsync();
        }
    }
}