using GodotXR.Application.DTOs.Request.Role;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Role;
using GodotXR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GodotXR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoleResponse>>>> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<RoleResponse>>.SuccessResponse(roles));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> GetById(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
                return NotFound(ApiResponse<RoleResponse>.FailureResponse($"Không tìm thấy role với id = {id}."));
            return Ok(ApiResponse<RoleResponse>.SuccessResponse(role));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> Create([FromBody] CreateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    .ToList();
                return BadRequest(ApiResponse<RoleResponse>.FailureResponse("Validation failed", errors));
            }
            try
            {
                var created = await _roleService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id },
                    ApiResponse<RoleResponse>.SuccessResponse(created, "Tạo role thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<RoleResponse>.FailureResponse(ex.Message));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<RoleResponse>>> Update(
            int id, [FromBody] UpdateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    .ToList();
                return BadRequest(ApiResponse<RoleResponse>.FailureResponse("Validation failed", errors));
            }
            try
            {
                var updated = await _roleService.UpdateAsync(id, request);
                if (updated == null)
                    return NotFound(ApiResponse<RoleResponse>.FailureResponse($"Không tìm thấy role với id = {id}."));
                return Ok(ApiResponse<RoleResponse>.SuccessResponse(updated, "Cập nhật role thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<RoleResponse>.FailureResponse(ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            var result = await _roleService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse.FailureResponse($"Không tìm thấy role với id = {id}."));
            return Ok(ApiResponse.SuccessResponse("Xóa role thành công"));
        }
    }
}