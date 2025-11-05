// Controllers/MaintenanceController.cs
using AssetManagement.API.Models;
using AssetManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AssetManagement.API.Controllers
{
    [Authorize]
    [Route("api/assets/{assetId}/maintenance")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMaintenanceRecords(string assetId)
        {
            var records = await _maintenanceService.GetMaintenanceRecordsForAssetAsync(assetId);
            return Ok(records);
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> AddMaintenanceRecord(string assetId, [FromBody] MaintenanceCreateDto recordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            long currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var record = new TrxMaintenanceRecord
            {
                LinkedAssetId = assetId,
                MaintenanceCost = recordDto.MaintenanceCost,
                MaintenanceType = recordDto.MaintenanceType,
                Comments = recordDto.Comments,
                Vendor = recordDto.Vendor,
                MaintenanceDate = recordDto.MaintenanceDate
            };

            var newRecord = await _maintenanceService.AddMaintenanceRecordAsync(record, currentUserId);
            return Ok(newRecord);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> UpdateMaintenanceRecord(string assetId, long id, [FromBody] MaintenanceUpdateDto recordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            long currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
            if (existingRecord == null || existingRecord.LinkedAssetId != assetId)
            {
                return NotFound();
            }

            // Map DTO to the existing entity
            existingRecord.MaintenanceCost = recordDto.MaintenanceCost;
            existingRecord.MaintenanceType = recordDto.MaintenanceType;
            existingRecord.Comments = recordDto.Comments;
            existingRecord.Vendor = recordDto.Vendor;
            existingRecord.MaintenanceDate = recordDto.MaintenanceDate;

            var updatedRecord = await _maintenanceService.UpdateMaintenanceRecordAsync(existingRecord, currentUserId);
            return Ok(updatedRecord);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> DeleteMaintenanceRecord(long id)
        {
            var success = await _maintenanceService.DeleteMaintenanceRecordAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}