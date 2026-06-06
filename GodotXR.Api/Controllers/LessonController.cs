using GodotXR.Application.DTOs.Request.Lesson;
using GodotXR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GodotXR.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _lessonService.GetAllAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("program/{programId:int}")]
        public async Task<IActionResult> GetByProgramId(int programId)
        {
            var response = await _lessonService.GetByProgramIdAsync(programId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _lessonService.GetByIdAsync(id);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLessonRequest request)
        {
            var response = await _lessonService.CreateAsync(request);
            return response.Success
                ? CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response)
                : BadRequest(response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLessonRequest request)
        {
            var response = await _lessonService.UpdateAsync(id, request);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _lessonService.DeleteAsync(id);
            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}