using GodotXR.Api.Contracts;
using GodotXR.Application.DTOs.Request.User;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.User;
using GodotXR.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GodotXR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult> Get([FromQuery] PaginationQuery query)
        {
            var data = await _userService.GetListUserAsync(
                query.PageNumber,
                query.PageSize);

            return Ok(new ApiResponse<PagedResponse<UserResponse>>
            {
                Success = true,
                Message = "OK",
                Data = data,
            });
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Invalid user id."
                });
            }

            var data = await _userService.GetUserByIdAsync(id);

            if (data is null)
            {
                return NotFound(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "User not found.",
                });
            }

            return Ok(new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "OK",
                Data = data,
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (request is null)
            {
                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Create user failed.",
                    Errors = new List<string> { "Request body is required." },
                });
            }

            var (ok, errors, data) = await _userService.CreateUserAsync(request);

            if (!ok || data is null)
            {
                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Create user failed.",
                    Errors = errors.ToList(),
                });
            }

            return Ok(new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "User created.",
                Data = data,
            });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(
            int id, [FromBody] UpdateUserRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Invalid user id."
                });
            }

            if (request is null)
            {
                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Create user failed.",
                    Errors = new List<string> { "Request body is required." },
                });
            }

            var (ok, notFound, errors, data) = await _userService.UpdateUserAsync(id, request);

            if (notFound)
            {
                return NotFound(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "User not found.",
                });
            }

            if (!ok || data is null)
            {
                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Update user failed.",
                    Errors = errors.ToList(),
                });
            }

            return Ok(new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "User updated.",
                Data = data,
            });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Invalid user id."
                });
            }

            var (ok, notFound, errors) = await _userService.DeleteUserAsync(id);

            if (notFound)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = false,
                });
            }

            if (!ok)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Delete user failed.",
                    Errors = errors.ToList(),
                    Data = false,
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "User deleted.",
                Data = true,
            });
        }
    }
}