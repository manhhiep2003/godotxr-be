using GodotXR.Application.DTOs.Response.FileUpload;
using GodotXR.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GodotXR.Api.Controllers
{
    public class UploadFilesRequest
    {
        public IFormFile Metadata { get; set; } = null!;
        public IFormFile Audio { get; set; } = null!;
    }

    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class FilesController : ControllerBase
    {
        private readonly IStorageService _storage;

        public FilesController(IStorageService storage)
        {
            _storage = storage;
        }

        [HttpPost]
        public async Task<ActionResult<UploadFilesResponse>> Upload(
            [FromForm] UploadFilesRequest request,
            CancellationToken ct)
        {
            if (request.Metadata is null || request.Metadata.Length == 0)
            {
                return BadRequest("Metadata file is required.");
            }

            if (request.Audio is null || request.Audio.Length == 0)
            {
                return BadRequest("Audio file is required.");
            }

            var folderId = Guid.NewGuid();

            var metadataObject = $"records/{folderId}/metadata.json";
            var audioObject = $"records/{folderId}/voice.wav";

            await using var metadataStream = request.Metadata.OpenReadStream();

            await _storage.UploadAsync(
                metadataStream,
                metadataObject,
                "application/json",
                ct);

            await using var audioStream = request.Audio.OpenReadStream();

            await _storage.UploadAsync(
                audioStream,
                audioObject,
                "audio/wav",
                ct);

            return Ok(new UploadFilesResponse(folderId));
        }
    }
}
