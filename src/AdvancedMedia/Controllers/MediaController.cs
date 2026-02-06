using AdvancedMedia.Services;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace AdvancedMedia.Controllers;

[ApiController]
[Route("[controller]")]
public class MediaController : ControllerBase
{
    private readonly BlobStorageService _blobService;

    public MediaController(BlobStorageService blobService)
    {
        _blobService = blobService;
    }

    [HttpPost("upload")]
    [DisableRequestSizeLimit] // Allow large uploads
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file");

        using var stream = file.OpenReadStream();
        var uri = await _blobService.UploadFileChunkedAsync(stream, file.FileName, file.ContentType);
        
        return Ok(new { Uri = uri });
    }

    [HttpGet("stream/{fileName}")]
    public async Task<IActionResult> StreamVideo(string fileName)
    {
        // 1. Get Blob Client
        var blobClient = _blobService.GetBlockBlobClient(fileName);
        if (!await blobClient.ExistsAsync()) return NotFound();

        var props = await blobClient.GetPropertiesAsync();
        var stream = await blobClient.OpenReadAsync();

        // 2. Return FileStreamResult
        // "EnableRangeProcessing = true" is the Magic. 
        // It handles the "Range: bytes=0-1024" header automatically.
        return File(stream, props.Value.ContentType, enableRangeProcessing: true);
    }

    [HttpGet("thumbnail/{fileName}")]
    public async Task<IActionResult> GetThumbnail(string fileName, int width = 200)
    {
        var stream = await _blobService.GetFileStreamAsync(fileName);
        if (stream == null) return NotFound();

        // 1. Decode Image
        using var originalBitmap = SKBitmap.Decode(stream);
        if (originalBitmap == null) return BadRequest("Not a valid image");

        // 2. Resize
        var height = (int)(originalBitmap.Height * ((float)width / originalBitmap.Width));
        var info = new SKImageInfo(width, height);
        using var resizedBitmap = originalBitmap.Resize(info, SKFilterQuality.Medium);
        using var image = SKImage.FromBitmap(resizedBitmap);
        
        // 3. Encode to PNG
        using var data = image.Encode(SKEncodedImageFormat.Png, 80);
        var memoryStream = new MemoryStream();
        data.SaveTo(memoryStream);
        memoryStream.Position = 0;

        return File(memoryStream, "image/png");
    }
}
