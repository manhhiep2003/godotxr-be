using GodotXR.Api.Contracts;
using GodotXR.Application.DTOs.Request.Analyze;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.DTOs.Response.Analyze;
using GodotXR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GodotXR.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class AnalyzeController : ControllerBase
    {
        private readonly IAnalyzeService _analyzeService;

        public AnalyzeController(IAnalyzeService analyzeService)
        {
            _analyzeService = analyzeService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Parent")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<AnalyzeResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult> Get([FromQuery] PaginationQuery query)
        {
            var data = await _analyzeService.GetListAnalyzeAsync(
                query.PageNumber,
                query.PageSize);

            return Ok(new ApiResponse<PagedResponse<AnalyzeResponse>>
            {
                Success = true,
                Message = "OK",
                Data = data,
            });
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Teacher,Parent")]
        [ProducesResponseType(typeof(ApiResponse<AnalyzeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AnalyzeResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<AnalyzeResponse>
                {
                    Success = false,
                    Message = "Invalid analyze id."
                });

            var data = await _analyzeService.GetAnalyzeByIdAsync(id);

            if (data == null)
                return NotFound(new ApiResponse<AnalyzeResponse>
                {
                    Success = false,
                    Message = "Analyze not found."
                });

            return Ok(new ApiResponse<AnalyzeResponse>
            {
                Success = true,
                Message = "OK",
                Data = data
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AnalyzeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AnalyzeResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create(
            [FromBody] CreateAnalyzeRequest request)
        {
            var (ok, errors, data) = await _analyzeService.CreateAnalyzeAsync(request);

            if (!ok)
            {
                return BadRequest(new ApiResponse<AnalyzeResponse>
                {
                    Success = false,
                    Message = "Create analyze failed.",
                    Errors = errors.ToList()
                });
            }

            return Ok(new ApiResponse<AnalyzeResponse>
            {
                Success = true,
                Message = "Analyze created.",
                Data = data
            });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AnalyzeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AnalyzeResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AnalyzeResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateAnalyzeRequest request)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<AnalyzeResponse>
                {
                    Success = false,
                    Message = "Invalid analyze id."
                });

            var (ok, notFound, errors, data) = await _analyzeService.UpdateAnalyzeAsync(id, request);

            if (notFound)
                return NotFound(new ApiResponse<AnalyzeResponse>
                {
                    Success = false,
                    Message = "Analyze not found."
                });

            if (!ok || data == null)
                return BadRequest(new ApiResponse<AnalyzeResponse>
                {
                    Success = false,
                    Message = "Update analyze failed.",
                    Errors = errors.ToList()
                });

            return Ok(new ApiResponse<AnalyzeResponse>
            {
                Success = true,
                Message = "Analyze updated.",
                Data = data
            });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid analyze id."
                });
            }

            var (ok, notFound, errors) = await _analyzeService.DeleteAnalyzeAsync(id);

            if (notFound)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Analyze not found.",
                    Data = false
                });
            }

            if (!ok)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Delete analyze failed.",
                    Errors = errors.ToList(),
                    Data = false
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Analyze deleted.",
                Data = true
            });
        }
    }
}
