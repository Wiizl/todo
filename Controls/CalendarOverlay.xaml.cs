using System.Globalization;

namespace TodoLists.Controls;

/// <summary>In-app month calendar. The week always starts on Monday, regardless of device settings.</summary>
public partial class CalendarOverlay : ContentView
{
    static readonly string[] WeekDays = { "Mo", "Tu", "We", "Th", "Fr", "Sa", "Su" };

    TaskCompletionSource<DateTime?>? _tcs;
    DateTime _month;      // first day of the month being shown
    DateTime _selected;
    bool _closing;

    public CalendarOverlay()
    {
        InitializeComponent();
        for (int i = 0; i < 7; i++)
        {
            var l = new Label
            {
                Text = WeekDays[i], FontSize = 12, FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
            };
            l.SetDynamicResource(Label.TextColorProperty, "TextMuted");
            WeekHeader.Add(l, i, 0);
        }
    }

    public bool IsOpen => IsVisible;

    static object Res(string key) => Application.Current!.Resources[key];

    /// <summary>Returns the chosen date, or null if cancelled.</summary>
    public async Task<DateTime?> PickAsync(DateTime initial)
    {
        if (_tcs != null) return null;
        _tcs = new TaskCompletionSource<DateTime?>();
        _closing = false;
        _selected = initial.Date;
        _month = new DateTime(initial.Year, initial.Month, 1);
        Render();

        Backdrop.Opacity = 0;
        Card.Opacity = 0;
        Card.Scale = 0.9;
        Card.TranslationY = 24;
        IsVisible = true;

        var task = _tcs.Task;
        await Task.WhenAll(
            Backdrop.FadeToAsync(1, 180),
            Card.FadeToAsync(1, 200),
            Card.ScaleToAsync(1, 260, Easing.CubicOut),
            Card.TranslateToAsync(0, 0, 260, Easing.CubicOut));
        return await task;
    }

    public void Cancel() => Close(null);

    void Render()
    {
        MonthLabel.Text = _month.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        DaysGrid.Children.Clear();

        int offset = ((int)_month.DayOfWeek + 6) % 7;      // Monday = 0
        int days = DateTime.DaysInMonth(_month.Year, _month.Month);
        var today = DateTime.Today;

        for (int d = 1; d <= days; d++)
        {
            int idx = offset + d - 1;
            var date = new DateTime(_month.Year, _month.Month, d);

            var btn = new Button
            {
                Text = d.ToString(CultureInfo.InvariantCulture),
                WidthRequest = 42, HeightRequest = 42, CornerRadius = 21, Padding = 0,
                FontSize = 15, HorizontalOptions = LayoutOptions.Center,
            };

            if (date == _selected)
            {
                btn.Background = (Brush)Res("PrimaryGradient");
                btn.TextColor = Colors.White;
                btn.FontAttributes = FontAttributes.Bold;
            }
            else if (date == today)
            {
                btn.BackgroundColor = ((Color)Res("Primary")).WithAlpha(0.16f);
                btn.TextColor = (Color)Res("Primary");
                btn.FontAttributes = FontAttributes.Bold;
            }
            else
            {
                btn.BackgroundColor = Colors.Transparent;
                btn.TextColor = (Color)Res("TextMain");
            }

            btn.Clicked += (_, _) => Close(date);
            DaysGrid.Add(btn, idx % 7, idx / 7);
        }
    }

    async void Close(DateTime? result)
    {
        if (_closing || _tcs == null) return;
        _closing = true;

        await Task.WhenAll(Backdrop.FadeToAsync(0, 150), Card.FadeToAsync(0, 150), Card.ScaleToAsync(0.95, 150));
        IsVisible = false;

        var tcs = _tcs;
        _tcs = null;
        tcs.TrySetResult(result);
    }

    void OnPrev(object? sender, EventArgs e) { _month = _month.AddMonths(-1); Render(); }
    void OnNext(object? sender, EventArgs e) { _month = _month.AddMonths(1); Render(); }
    void OnToday(object? sender, EventArgs e) => Close(DateTime.Today);
    void OnCancel(object? sender, EventArgs e) => Close(null);
    void OnBackdropTapped(object? sender, TappedEventArgs e) => Close(null);
    void OnCardTapped(object? sender, TappedEventArgs e) { /* swallow so the backdrop doesn't dismiss */ }
}
