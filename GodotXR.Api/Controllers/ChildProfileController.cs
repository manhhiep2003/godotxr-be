using GodotXR.Api.Contracts;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.ChildProfile;
using GodotXR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GodotXR.Api.Controllers
{
    [ApiController]
    [Route("api/child-profile")]
    [Authorize(Roles = "Admin,Teacher,Parent")]
    public class ChildProfileController : ControllerBase
    {
        private readonly IChildProfileService _childProfileService;

        public ChildProfileController(IChildProfileService childProfileService)
        {
            _childProfileService = childProfileService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ChildProfileResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult> Get([FromQuery] PaginationQuery query)
        {
            var data = await _childProfileService.GetListChildProfileAsync(
                query.PageNumber,
                query.PageSize);

            return Ok(new ApiResponse<PagedResponse<ChildProfileResponse>>
            {
                Success = true,
                Message = "OK",
                Data = data,
            });
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ChildProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ChildProfileResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<ChildProfileResponse>
                {
                    Success = false,
                    Message = "Invalid child profile id."
                });

            var data = await _childProfileService.GetChildProfileByIdAsync(id);

            if (data == null)
                return NotFound(new ApiResponse<ChildProfileResponse>
                {
                    Success = false,
                    Message = "Child profile not found."
                });

            return Ok(new ApiResponse<ChildProfileResponse>
            {
                Success = true,
                Message = "OK",
                Data = data
            });
        }
    }
}
