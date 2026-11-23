#if ANDROID
using AGraphics = Android.Graphics;
#endif

namespace TodoLists.Services;

/// <summary>Stores category icon images next to data.json (FileSystem.AppDataDirectory).</summary>
public static class ImageService
{
    public const int MaxSize = 512; // longest side in pixels

    public static string FullPath(string fileName) => Path.Combine(FileSystem.AppDataDirectory, fileName);

    /// <summary>Downsizes the picked image (aspect ratio kept, EXIF rotation applied), saves it as JPEG
    /// in the data folder and returns the new file name.</summary>
    public static async Task<string> SaveCategoryImageAsync(FileResult picked, string categoryId)
    {
        var tmp = Path.Combine(FileSystem.CacheDirectory, Guid.NewGuid().ToString("N") + ".tmp");
        var fileName = $"cat_{categoryId}_{Guid.NewGuid().ToString("N")[..8]}.jpg";
        var dest = FullPath(fileName);

        try
        {
            using (var src = await picked.OpenReadAsync())
            using (var fs = File.Create(tmp))
                await src.CopyToAsync(fs);

            await Task.Run(() => ResizeToJpeg(tmp, dest));
            return fileName;
        }
        catch
        {
            TryDelete(dest);
            throw;
        }
        finally
        {
            TryDelete(tmp);
        }
    }

    public static void Delete(string? fileName)
    {
        if (!string.IsNullOrEmpty(fileName)) TryDelete(FullPath(fileName));
    }

    static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

#if ANDROID
    static void ResizeToJpeg(string srcPath, string destPath)
    {
        // 1) read dimensions only
        var bounds = new AGraphics.BitmapFactory.Options { InJustDecodeBounds = true };
        AGraphics.BitmapFactory.DecodeFile(srcPath, bounds);
        int w = bounds.OutWidth, h = bounds.OutHeight;
        if (w <= 0 || h <= 0) throw new InvalidOperationException("Not a valid image.");

        // 2) decode with sub-sampling so huge photos don't blow memory
        int sample = 1;
        while (Math.Max(w, h) / (sample * 2) >= MaxSize) sample *= 2;
        var opts = new AGraphics.BitmapFactory.Options { InSampleSize = sample };
        var bmp = AGraphics.BitmapFactory.DecodeFile(srcPath, opts)
                  ?? throw new InvalidOperationException("Could not decode image.");

        // 3) EXIF rotation + scale down (aspect ratio preserved)
        int orientation = 1;
        try { orientation = new Android.Media.ExifInterface(srcPath).GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 1); }
        catch { }

        var m = new AGraphics.Matrix();
        switch (orientation)
        {
            case 3: m.PostRotate(180); break;
            case 6: m.PostRotate(90); break;
            case 8: m.PostRotate(270); break;
        }
        float scale = Math.Min(1f, (float)MaxSize / Math.Max(bmp.Width, bmp.Height));
        m.PostScale(scale, scale);

        var scaled = AGraphics.Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, m, true)!;
        if (!ReferenceEquals(scaled, bmp)) bmp.Recycle();

        // 4) flatten onto white (JPEG has no transparency) and save
        var flat = AGraphics.Bitmap.CreateBitmap(scaled.Width, scaled.Height, AGraphics.Bitmap.Config.Argb8888!)!;
        using (var canvas = new AGraphics.Canvas(flat))
        {
            canvas.DrawColor(AGraphics.Color.White);
            canvas.DrawBitmap(scaled, 0, 0, null);
        }
        scaled.Recycle();

        using (var fs = File.Create(destPath))
            flat.Compress(AGraphics.Bitmap.CompressFormat.Jpeg!, 88, fs);
        flat.Recycle();
    }
#else
    static void ResizeToJpeg(string srcPath, string destPath) => throw new PlatformNotSupportedException();
#endif
}
