using AssetManagement.API.Models;
using AssetManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AssetManagement.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly AssetManagementDbContext _context;

        public AssetsController(IAssetService assetService, AssetManagementDbContext context)
        {
            _assetService = assetService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            var assets = await _assetService.GetAllAssetsAsync();
            return Ok(assets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsset(string id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();
            return Ok(asset);
        }

        [HttpGet("myAssets")]
        public async Task<IActionResult> GetAssetByCurrentUser()
        {
            long currentUserId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var assets = await _assetService.GetAssetsByUserIdAsync(currentUserId);
            return Ok(assets);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsset([FromBody] AssetCreateDto assetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Map the DTO to the TrxAsset entity
            var asset = new TrxAsset
            {
                AssetName = assetDto.AssetName,
                Description = assetDto.Description,
                ResponsiblePersonId = assetDto.ResponsiblePersonId,
                Category = assetDto.Category,
                Subcategory = assetDto.Subcategory
            };

            long currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var newAsset = await _assetService.CreateAssetAsync(asset, currentUserId);
            return CreatedAtAction(nameof(GetAsset), new { id = newAsset.Id }, newAsset);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(string id, [FromBody] AssetUpdateDto assetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Retrieve the current user's ID from the JWT token claims
            long currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Fetch the existing entity from the database
            var existingAsset = await _assetService.GetAssetByIdAsync(id);
            if (existingAsset == null)
            {
                return NotFound();
            }

            // Map the updatable properties from the DTO to the entity
            existingAsset.AssetName = assetDto.AssetName;
            existingAsset.Description = assetDto.Description;
            existingAsset.ResponsiblePersonId = assetDto.ResponsiblePersonId;
            existingAsset.Category = assetDto.Category;
            existingAsset.Subcategory = assetDto.Subcategory;
            existingAsset.Status = assetDto.Status;

            // Call the service to perform the update
            await _assetService.UpdateAssetAsync(existingAsset, currentUserId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAsset(string id)
        {
            // Get current user's ID from the token claims
            var currentUserId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get the user's details from the database
            var currentUser = await _context.MstUsers.FindAsync(currentUserId);
            if (currentUser == null)
            {
                return Unauthorized(); // Should not happen if [Authorize] works
            }

            // Get the IT Department's ID
            var itDepartment = await _context.MstDepartments
                                             .FirstOrDefaultAsync(d => d.Id == 2);

            // If for some reason the IT Department isn't in the database, deny deletion for safety.
            if (itDepartment == null)
            {
                // Log an error and forbid for safety.
                return Forbid();
            }

            // New Logic: A "user" in the "IT Department" cannot delete.
            if (currentUser.Title.Equals("user", StringComparison.OrdinalIgnoreCase) && currentUser.DepartmentId == itDepartment.Id)
            {
                return Forbid("Users in the IT Department do not have permission to delete assets.");
            }

            var success = await _assetService.DeleteAssetAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("export")]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> ExportAssets()
        {
            // Requirement 12: Add role check for "Headquarters_Manager"
            // For example: if (!User.IsInRole("Headquarters_Manager")) return Forbid();

            var assets = await _assetService.GetAllAssetsAsync();
            // Here, you would typically use a library like CsvHelper or EPPlus to generate a file.
            return Ok(assets);
        }

        [HttpPost("AskForReview/{id}")]
        public async Task<IActionResult> AskForReview(string id)
        {
            long currentUserId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var asset = await _assetService.GetAssetByIdAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            // Ensure the user asking for review is the one who requested the asset
            // and the asset is in the correct state.
            if (asset.RequesterId != currentUserId)
            {
                return Forbid();
            }

            if (asset.Status != "New")
            {
                return BadRequest("Asset is not in 'New' status.");
            }

            asset.Status = "Under Review";
            await _assetService.UpdateAssetAsync(asset, currentUserId);

            return NoContent();
        }
    }
}