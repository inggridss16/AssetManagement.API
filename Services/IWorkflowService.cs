using System.Threading.Tasks;

namespace AssetManagement.API.Services
{
    public interface IWorkflowService
    {
        Task StartApprovalProcessAsync(string assetId, long requesterId);
        Task SubmitApprovalAsync(string assetId, long approverId, bool isApproved, string comments);
    }
}