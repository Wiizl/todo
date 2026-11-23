using TodoLists.Services;

namespace TodoLists;

public partial class App : Application
{
    static readonly Dictionary<string, string> Light = new()
    {
        ["PageBg"] = "#F5F6FB", ["CardBg"] = "#FFFFFF", ["Surface2"] = "#EEF0F8",
        ["TextMain"] = "#1B1C2E", ["TextMuted"] = "#8B8FA8",
        ["Primary"] = "#6C5CE7", ["Danger"] = "#E5484D", ["DangerBg"] = "#FDECEE",
        ["Backdrop"] = "#990B0C1A",
    };

    static readonly Dictionary<string, string> Dark = new()
    {
        ["PageBg"] = "#0E0F1A", ["CardBg"] = "#1A1B2B", ["Surface2"] = "#26283D",
        ["TextMain"] = "#F2F3FA", ["TextMuted"] = "#8D90AB",
        ["Primary"] = "#8B7DFF", ["Danger"] = "#FF6369", ["DangerBg"] = "#3A1A20",
        ["Backdrop"] = "#B3000000",
    };

    public App()
    {
        InitializeComponent();
        ApplyTheme();
        RequestedThemeChanged += (_, _) => ApplyTheme();
        Store.Load();
    }

    void ApplyTheme()
    {
        var palette = RequestedTheme == AppTheme.Dark ? Dark : Light;
        foreach (var kv in palette)
            Resources[kv.Key] = Color.FromArgb(kv.Value);
    }

    protected override Window CreateWindow(IActivationState? activationState)
      //  => new Window(new MainTabs());
         => new Window(new SplashPage());
}
