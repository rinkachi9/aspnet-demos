using SkiaSharp;

namespace AdvancedMedia.Services;

public class WatermarkService
{
    public Stream ApplyWatermark(Stream imageStream, string watermarkText)
    {
        using var originalBitmap = SKBitmap.Decode(imageStream);
        if (originalBitmap == null) throw new ArgumentException("Invalid image stream");

        using var surface = SKSurface.Create(new SKImageInfo(originalBitmap.Width, originalBitmap.Height));
        var canvas = surface.Canvas;

        // Draw original image
        canvas.DrawBitmap(originalBitmap, 0, 0);

        // Configure Text Paint
        using var paint = new SKPaint
        {
            Color = SKColors.White.WithAlpha(128), // Semi-transparent white
            IsAntialias = true,
            TextSize = originalBitmap.Height * 0.1f, // 10% of height
            TextAlign = SKTextAlign.Center
        };

        // Draw Text (Centered)
        var x = originalBitmap.Width / 2;
        var y = originalBitmap.Height / 2 + paint.TextSize / 2;
        
        canvas.DrawText(watermarkText, x, y, paint);

        // Return as PNG stream
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        
        var outputStream = new MemoryStream();
        data.SaveTo(outputStream);
        outputStream.Position = 0;
        
        return outputStream;
    }
}
