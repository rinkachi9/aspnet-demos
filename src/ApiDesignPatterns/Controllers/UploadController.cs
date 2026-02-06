using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace ApiDesignPatterns.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private const long MaxFileSize = 1024 * 1024 * 1024; // 1GB

    [HttpPost("stream")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
    public async Task<IActionResult> UploadStream()
    {
        if (!Request.HasFormContentType || 
            !MediaTypeHeaderValue.TryParse(Request.ContentType, out var mediaTypeHeader) || 
            string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
        {
            return new UnsupportedMediaTypeResult();
        }

        var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, Request.Body);
        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

            if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                !string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                // Process File Section
                // In real app: Stream to CloudStorage or Disk
                // Simulating streaming read
                
                var buffer = new byte[8192];
                long totalBytes = 0;
                while (true)
                {
                    var read = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) break;
                    totalBytes += read;
                }
                
                return Ok(new { FileName = contentDisposition.FileName.Value, Size = totalBytes, Message = "Uploaded via Streaming" });
            }

            section = await reader.ReadNextSectionAsync();
        }

        return BadRequest("No file found in request");
    }
}
