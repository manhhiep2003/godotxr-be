using GodotXR.Application.DTOs.Request.Program;
using GodotXR.Application.DTOs.Response;
using GodotXR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GodotXR.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProgramController : ControllerBase
    {
        private readonly IProgramService _programService;

        public ProgramController(IProgramService programService)
        {
            _programService = programService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _programService.GetAllAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _programService.GetByIdAsync(id);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProgramRequest request)
        {
            var response = await _programService.CreateAsync(request);
            return response.Success
                ? CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response)
                : BadRequest(response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProgramRequest request)
        {
            var response = await _programService.UpdateAsync(id, request);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _programService.DeleteAsync(id);
            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}