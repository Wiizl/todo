#if ANDROID
using Android.App;
using Android.Content;
using Android.Provider;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using Microsoft.Maui.ApplicationModel;
#endif

namespace TodoLists.Services;

/// <summary>Opens Android's photo picker (all photos + albums) and returns the chosen image.</summary>
public static class GalleryPicker
{
#if ANDROID
    static TaskCompletionSource<Android.Net.Uri?>? _tcs;
    static ActivityResultLauncher? _launcher;

    /// <summary>Call once from MainActivity.OnCreate.</summary>
    public static void Register(ComponentActivity activity)
        => _launcher = activity.RegisterForActivityResult(new ActivityResultContracts.StartActivityForResult(), new ResultCallback());

    /// <summary>Kept only so an old MainActivity.OnActivityResult override still compiles. Does nothing.</summary>
    public static void OnResult(int requestCode, Result resultCode, Intent? data) { }

    sealed class ResultCallback : Java.Lang.Object, IActivityResultCallback
    {
        public void OnActivityResult(Java.Lang.Object? result)
        {
            Android.Net.Uri? uri = null;
            if (result is ActivityResult r && r.ResultCode == (int)Result.Ok)
                uri = r.Data?.Data;

            var tcs = _tcs;
            _tcs = null;
            tcs?.TrySetResult(uri);
        }
    }
#endif

    /// <summary>Returns a copy of the picked image in the cache folder, or null if cancelled.</summary>
    public static async Task<FileResult?> PickImageAsync()
    {
#if ANDROID
        var activity = Platform.CurrentActivity;
        if (activity == null || _launcher == null)
            return await FilePicker.Default.PickAsync(new PickOptions { FileTypes = FilePickerFileType.Images });

        // never let an old, unanswered request block a new one
        _tcs?.TrySetResult(null);

        Intent intent = OperatingSystem.IsAndroidVersionAtLeast(33)
            ? new Intent(MediaStore.ActionPickImages)                                    // system photo picker
            : new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri); // gallery app

        var tcs = new TaskCompletionSource<Android.Net.Uri?>();
        _tcs = tcs;
        try
        {
            _launcher.Launch(intent);
        }
        catch
        {
            _tcs = null;
            throw;
        }

        var uri = await tcs.Task;
        if (uri == null) return null;

        var tmp = Path.Combine(FileSystem.CacheDirectory, $"pick_{Guid.NewGuid():N}.img");
        using (var input = activity.ContentResolver!.OpenInputStream(uri) ?? throw new IOException("Could not open image."))
        using (var fs = File.Create(tmp))
            await input.CopyToAsync(fs);

        return new FileResult(tmp);
#else
        return await FilePicker.Default.PickAsync(new PickOptions { FileTypes = FilePickerFileType.Images });
#endif
    }
}
