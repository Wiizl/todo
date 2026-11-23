using Android.App;
using Android.Content.PM;
using Android.OS;

namespace TodoLists;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity {
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // en-GB = English text, week starts on Monday
        //Java.Util.Locale.Default = new Java.Util.Locale("en", "GB");
        base.OnCreate(savedInstanceState);
    }
}


