using System.Globalization;
using TodoLists.Models;
using TodoLists.Services;

namespace TodoLists;

public partial class EventEditPage : ContentPage
{
    readonly EventItem _event;
    readonly bool _isNew;
    DateTime _date;
    TimeSpan _time;

    public EventEditPage(EventItem ev, bool isNew)
    {
        InitializeComponent();
        _event = ev;
        _isNew = isNew;

        HeaderLabel.Text = isNew ? "New event" : "Edit event";
        TitleEntry.Text = ev.Title;
        PlaceEntry.Text = ev.Place;

        if (isNew)
        {
            // default: today, next full hour
            var next = DateTime.Now.AddHours(1);
            _date = next.Date;
            _time = new TimeSpan(next.Hour, 0, 0);
        }
        else
        {
            _date = ev.When.Date;
            _time = new TimeSpan(ev.When.Hour, ev.When.Minute, 0);
        }
        UpdateDateLabel();
        UpdateTimeLabel();
    }

    void UpdateTimeLabel()
        => TimeLabel.Text = $"{_time.Hours:00}:{_time.Minutes:00}";

    void UpdateDateLabel()
        => DateLabel.Text = _date.ToString("ddd, d MMM yyyy", CultureInfo.InvariantCulture);

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        KeyboardHelper.Hide();
    }

    protected override bool OnBackButtonPressed()
    {
        if (Calendar.IsOpen) { Calendar.Cancel(); return true; }
        if (Clock.IsOpen) { Clock.Cancel(); return true; }
        return base.OnBackButtonPressed();
    }

    static object Res(string key) => Application.Current!.Resources[key];

    async void OnDateTapped(object? sender, TappedEventArgs e)
    {
        TitleEntry.Unfocus();
        PlaceEntry.Unfocus();
        KeyboardHelper.Hide();

        var picked = await Calendar.PickAsync(_date);
        if (picked.HasValue)
        {
            _date = picked.Value;
            UpdateDateLabel();
        }
    }

    async void OnTimeTapped(object? sender, TappedEventArgs e)
    {
        TitleEntry.Unfocus();
        PlaceEntry.Unfocus();
        KeyboardHelper.Hide();

        var picked = await Clock.PickAsync(_time);
        if (picked.HasValue)
        {
            _time = picked.Value;
            UpdateTimeLabel();
        }
    }

    async void OnSave(object? sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim();
        if (string.IsNullOrEmpty(title))
        {
            TitleBorder.Stroke = new SolidColorBrush((Color)Res("Danger"));
            ErrorLabel.Text = "Please enter a title.";
            ErrorLabel.IsVisible = true;
            return;
        }

        _event.Title = title;
        _event.When = _date.Date + _time;
        _event.Place = PlaceEntry.Text?.Trim() ?? "";

        if (_isNew) Store.Events.Add(_event);
        Store.Save();

        KeyboardHelper.Hide();
        await Navigation.PopAsync();
    }

    async void OnBack(object? sender, EventArgs e)
    {
        KeyboardHelper.Hide();
        await Navigation.PopAsync();
    }

    void OnBackgroundTapped(object? sender, TappedEventArgs e)
    {
        TitleEntry.Unfocus();
        PlaceEntry.Unfocus();
        KeyboardHelper.Hide();
    }

    void OnFieldFocused(object? sender, FocusEventArgs e)
    {
        if ((sender as VisualElement)?.Parent is Border b)
            b.Stroke = new SolidColorBrush((Color)Res("Primary"));
        ErrorLabel.IsVisible = false;
    }

    void OnFieldUnfocused(object? sender, FocusEventArgs e)
    {
        if ((sender as VisualElement)?.Parent is Border b)
            b.Stroke = Colors.Transparent;
    }
}
