namespace TodoLists;

/// <summary>Animated launch screen: pop-in, wink + sparkles, hedgehog lifts the list, fade into the app.</summary>
public partial class SplashPage : ContentPage
{
    const double TotalMs = 3100;
    double _size;
    bool _started, _done;

    public SplashPage() => InitializeComponent();

    static double Lerp(double a, double b, double t) => a + (b - a) * t;

    void OnSizeChanged(object? sender, EventArgs e)
    {
        if (_started || Root.Width <= 0 || Root.Height <= 0) return;
        _started = true;

        _size = Math.Min(Root.Width * 0.92, Root.Height * 0.60);
        Stage.WidthRequest = Stage.HeightRequest = _size;

        var pageBg = Application.Current?.Resources.TryGetValue("PageBg", out var c) == true && c is Color col
            ? col : Color.FromArgb("#F5F6FB");
        Wipe.Color = pageBg;

        Stage.Scale = 0.8;
        Stage.TranslationY = _size * 40 / 360;
        Play();
    }

    void Play()
    {
        var timeline = new Animation();
        void At(double start, double dur, Action<double> step, Easing? ease = null)
            => timeline.Add(start / TotalMs, (start + dur) / TotalMs, new Animation(step, 0, 1, ease ?? Easing.Linear));

        double dipPx = _size * 0.012;

        // 0 - 550 ms: solid colour -> gradient, hedgehog pops in
        At(0, 400, v => GradientLayer.Opacity = v);
        At(0, 350, v => Stage.Opacity = v, Easing.CubicOut);
        At(0, 550, v => Stage.Scale = Lerp(0.8, 1, v), Easing.SpringOut);
        At(0, 550, v => Stage.TranslationY = Lerp(_size * 40 / 360, 0, v), Easing.CubicOut);

        // 750 - 1200 ms: wink, sparkles flash in then out
        At(750, 450, v => { EyeOpen.IsVisible = v >= 1; EyeWink.IsVisible = v < 1; });
        At(750, 150, v => Sparkle.Opacity = v);
        At(750, 280, v => Sparkle.Scale = Lerp(0.6, 1, v), Easing.SpringOut);
        At(1200, 150, v => Sparkle.Opacity = 1 - v);

        // 1550 - 2070 ms: small dip, then lift the list up in front of the face
        At(1550, 140, v => { Paper.TranslationY = dipPx * v; Paper.Scale = Lerp(1, 0.985, v); }, Easing.CubicOut);
        At(1690, 380, v =>
        {
            Paper.TranslationY = Lerp(dipPx, -dipPx, v);
            Paper.Scale = Lerp(0.985, 1.06, v);
            Paper.Rotation = 4.5 * v;
        }, Easing.SpringOut);

        // 2820 - 3100 ms: fade to the app
        At(2820, 280, v => Wipe.Opacity = v, Easing.CubicIn);

        timeline.Commit(this, "Splash", 16, (uint)TotalMs, Easing.Linear, (_, _) => GoToApp());
    }

    void OnTapped(object? sender, TappedEventArgs e)
    {
        this.AbortAnimation("Splash");   // tap to skip
        GoToApp();
    }

    void GoToApp()
    {
        if (_done) return;
        _done = true;
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null) window.Page = new MainTabs();
    }
}
