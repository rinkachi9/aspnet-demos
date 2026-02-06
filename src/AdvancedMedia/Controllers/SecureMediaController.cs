using AdvancedMedia.Services;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedMedia.Controllers;

[ApiController]
[Route("[controller]")]
public class SecureMediaController : ControllerBase
{
    private readonly SasTokenService _sasService;
    private readonly WatermarkService _watermarkService;
    private readonly BlobStorageService _blobService;

    public SecureMediaController(
        SasTokenService sasService, 
        WatermarkService watermarkService,
        BlobStorageService blobService)
    {
        _sasService = sasService;
        _watermarkService = watermarkService;
        _blobService = blobService;
    }

    [HttpGet("auth-url/{fileName}")]
    public IActionResult GetSasUrl(string fileName)
    {
        try 
        {
            // In prod: Check User Permissions here first!
            var uri = _sasService.GenerateReadSasUri(fileName, TimeSpan.FromMinutes(5));
            return Ok(new { SasUri = uri.ToString(), ExpiresInSeconds = 300 });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("watermark/{fileName}")]
    public async Task<IActionResult> GetWatermarkedImage(string fileName, string text = "CONFIDENTIAL")
    {
        var stream = await _blobService.GetFileStreamAsync(fileName);
        if (stream == null) return NotFound();

        try
        {
            var watermarkedStream = _watermarkService.ApplyWatermark(stream, text);
            return File(watermarkedStream, "image/png");
        }
        catch (Exception)
        {
            return BadRequest("Could not process image");
        }
    }

    [HttpGet("upload-status/{fileName}")]
    public async Task<IActionResult> GetUploadStatus(string fileName)
    {
        // Check uncommitted blocks to see partial upload state
        var blockBlobClient = _blobService.GetBlockBlobClient(fileName);
        
        try 
        {
            var blockList = await blockBlobClient.GetBlockListAsync(BlockListTypes.All);
            return Ok(new 
            { 
                CommittedCount = blockList.Value.CommittedBlocks.Count(),
                UncommittedCount = blockList.Value.UncommittedBlocks.Count(),
                TotalSize = blockList.Value.CommittedBlocks.Sum(b => b.Size) + blockList.Value.UncommittedBlocks.Sum(b => b.Size)
            });
        }
        catch
        {
            return NotFound("Upload not found or file does not exist yet.");
        }
    }

    [HttpPost("queue-processing/{fileName}")]
    public IActionResult QueueForTranscoding(string fileName)
    {
        MediaBackgroundProcessor.ProcessingQueue.Enqueue(fileName);
        return Accepted(new { Message = "Queued for processing" });
    }
}
