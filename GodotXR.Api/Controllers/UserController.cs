using GodotXR.Application.DTOs.Request.User;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.User;
using GodotXR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GodotXR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponse>>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(users));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse<UserResponse>.FailureResponse($"Không tìm thấy user với id = {id}."));

            return Ok(ApiResponse<UserResponse>.SuccessResponse(user));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponse>>> Create([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    .ToList();
                return BadRequest(ApiResponse<UserResponse>.FailureResponse("Validation failed", errors));
            }

            try
            {
                var created = await _userService.CreateAsync(request);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.Id },
                    ApiResponse<UserResponse>.SuccessResponse(created, "Tạo user thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserResponse>.FailureResponse(ex.Message));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> Update(
            int id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    .ToList();
                return BadRequest(ApiResponse<UserResponse>.FailureResponse("Validation failed", errors));
            }
            try
            {
                var updated = await _userService.UpdateAsync(id, request);
                if (updated == null)
                    return NotFound(ApiResponse<UserResponse>.FailureResponse($"Không tìm thấy user với id = {id}."));

                return Ok(ApiResponse<UserResponse>.SuccessResponse(updated, "Cập nhật user thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserResponse>.FailureResponse(ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse.FailureResponse($"Không tìm thấy user với id = {id}."));

            return Ok(ApiResponse.SuccessResponse("Xóa user thành công"));
        }
    }
}