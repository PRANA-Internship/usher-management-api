using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UMS.Application.Common.Interfaces;

namespace UMS.api.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public sealed class FilesController(IFileStorageService fileStorage) : ControllerBase
    {

        [HttpPost("uploads")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> Upload(
            IFormFile file,
            CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest("File is required.");

            var path = await fileStorage.UploadFileAsync(
                fileStream: file.OpenReadStream(),
                fileName: file.FileName,
                contentType: file.ContentType,
                folder: "tmp",
                ct: ct);

            return Ok(new
            {
                filePath = path,
                contentType = file.ContentType,
                fileName = file.FileName
            });
        }


        [HttpGet("download")]
        public async Task<IActionResult> Download(
            [FromQuery] string objectName,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return BadRequest("objectName is required.");

            var exists = await fileStorage.FileExistsAsync(objectName, ct);
            if (!exists)
                return NotFound("File not found.");

            var stream = await fileStorage.DownloadFileAsync(objectName, ct);

            var extension = Path.GetExtension(objectName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return File(stream, contentType);
        }


        [HttpGet("exists")]
        public async Task<IActionResult> Exists(
            [FromQuery] string objectName,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return BadRequest("objectName is required.");

            var exists = await fileStorage.FileExistsAsync(objectName, ct);
            return Ok(new { exists });
        }


        [HttpPost("move")]
        public async Task<IActionResult> Move(
            [FromBody] MoveFileRequest request,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.SourcePath) ||
                string.IsNullOrWhiteSpace(request.DestinationFolder))
                return BadRequest("SourcePath and DestinationFolder are required.");

            var exists = await fileStorage.FileExistsAsync(request.SourcePath, ct);
            if (!exists)
                return NotFound("Source file not found.");

            var newPath = await fileStorage.MoveFileAsync(
                request.SourcePath,
                request.DestinationFolder,
                ct);

            return Ok(new { filePath = newPath });
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(
            [FromQuery] string objectName,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return BadRequest("objectName is required.");

            var exists = await fileStorage.FileExistsAsync(objectName, ct);
            if (!exists)
                return NotFound("File not found.");

            await fileStorage.DeleteAsync(objectName, ct);
            return Ok(new { message = "File deleted successfully." });
        }
    }

    public record MoveFileRequest(string SourcePath, string DestinationFolder);
}