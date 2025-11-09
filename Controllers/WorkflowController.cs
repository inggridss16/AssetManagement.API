using AssetManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AssetManagement.API.Controllers
{
    [Authorize(Roles = "manager")]
    [Route("api/workflow")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            long currentUserId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var assets = await _workflowService.GetPendingApprovalsAsync(currentUserId);
            return Ok(assets);
        }
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveOrReject([FromBody] ApprovalDto approvalDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            long currentUserId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _workflowService.SubmitApprovalAsync(approvalDto.AssetId, currentUserId, approvalDto.IsApproved, approvalDto.Comments);

            return Ok();
        }
    }

    public class ApprovalDto
    {
        public string AssetId { get; set; }
        public bool IsApproved { get; set; }
        public string Comments { get; set; }
    }
}