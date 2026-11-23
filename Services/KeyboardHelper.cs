namespace TodoLists.Services;

public static class KeyboardHelper
{
    public static void Hide()
    {
#if ANDROID
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        var view = activity?.CurrentFocus ?? activity?.Window?.DecorView;
        if (view != null &&
            activity?.GetSystemService(Android.Content.Context.InputMethodService)
                is Android.Views.InputMethods.InputMethodManager imm)
        {
            imm.HideSoftInputFromWindow(view.WindowToken, Android.Views.InputMethods.HideSoftInputFlags.None);
        }
#endif
    }
}
