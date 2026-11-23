using System.Globalization;

namespace TodoLists.Controls;

/// <summary>In-app 24-hour time picker (two scrolling columns: hours and minutes).</summary>
public partial class TimeOverlay : ContentView
{
    const double RowHeight = 44, RowSpacing = 4, ViewportHeight = 220;

    readonly List<Button> _hours = new(), _minutes = new();
    TaskCompletionSource<TimeSpan?>? _tcs;
    int _hour, _minute;
    bool _closing;

    public TimeOverlay()
    {
        InitializeComponent();
        for (int h = 0; h < 24; h++) { int v = h; _hours.Add(MakeButton(v, HourHost, () => { _hour = v; Refresh(); })); }
        for (int m = 0; m < 60; m += 5) { int v = m; _minutes.Add(MakeButton(v, MinuteHost, () => { _minute = v; Refresh(); })); }
    }

    public bool IsOpen => IsVisible;

    static object Res(string key) => Application.Current!.Resources[key];

    static Button MakeButton(int value, VerticalStackLayout host, Action onTap)
    {
        var b = new Button
        {
            Text = value.ToString("00", CultureInfo.InvariantCulture),
            HeightRequest = RowHeight, CornerRadius = 14, FontSize = 17, Padding = 0,
        };
        b.Clicked += (_, _) => onTap();
        host.Children.Add(b);
        return b;
    }

    void Refresh()
    {
        ValueLabel.Text = $"{_hour:00}:{_minute:00}";
        Style(_hours, _hour, 1);
        Style(_minutes, _minute, 5);
    }

    static void Style(List<Button> buttons, int selected, int step)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            var b = buttons[i];
            if (i * step == selected)   // a minute that isn't a multiple of 5 stays unhighlighted until changed
            {
                b.Background = (Brush)Res("PrimaryGradient");
                b.TextColor = Colors.White;
                b.FontAttributes = FontAttributes.Bold;
            }
            else
            {
                b.Background = new SolidColorBrush((Color)Res("Surface2"));
                b.TextColor = (Color)Res("TextMain");
                b.FontAttributes = FontAttributes.None;
            }
        }
    }

    static double OffsetFor(int index) => Math.Max(0, index * (RowHeight + RowSpacing) - (ViewportHeight - RowHeight) / 2);

    /// <summary>Returns the chosen time, or null if cancelled.</summary>
    public async Task<TimeSpan?> PickAsync(TimeSpan initial)
    {
        if (_tcs != null) return null;
        _tcs = new TaskCompletionSource<TimeSpan?>();
        _closing = false;
        _hour = initial.Hours;
        _minute = initial.Minutes;
        Refresh();

        Backdrop.Opacity = 0;
        Card.Opacity = 0;
        Card.Scale = 0.9;
        Card.TranslationY = 24;
        IsVisible = true;

        Dispatcher.Dispatch(async () =>
        {
            await Task.Delay(60);
            await HourScroll.ScrollToAsync(0, OffsetFor(_hour), false);
            await MinuteScroll.ScrollToAsync(0, OffsetFor(Math.Min(11, (_minute + 2) / 5)), false);
        });

        var task = _tcs.Task;
        await Task.WhenAll(
            Backdrop.FadeToAsync(1, 180),
            Card.FadeToAsync(1, 200),
            Card.ScaleToAsync(1, 260, Easing.CubicOut),
            Card.TranslateToAsync(0, 0, 260, Easing.CubicOut));
        return await task;
    }

    public void Cancel() => Close(null);

    async void Close(TimeSpan? result)
    {
        if (_closing || _tcs == null) return;
        _closing = true;

        await Task.WhenAll(Backdrop.FadeToAsync(0, 150), Card.FadeToAsync(0, 150), Card.ScaleToAsync(0.95, 150));
        IsVisible = false;

        var tcs = _tcs;
        _tcs = null;
        tcs.TrySetResult(result);
    }

    void OnOk(object? sender, EventArgs e) => Close(new TimeSpan(_hour, _minute, 0));
    void OnCancel(object? sender, EventArgs e) => Close(null);
    void OnBackdropTapped(object? sender, TappedEventArgs e) => Close(null);
    void OnCardTapped(object? sender, TappedEventArgs e) { /* swallow so the backdrop doesn't dismiss */ }
}
