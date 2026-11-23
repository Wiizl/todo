using System.Globalization;
using TodoLists.Models;
using TodoLists.Services;

namespace TodoLists;

public partial class BeerEditPage : ContentPage
{
    readonly Beer _beer;
    readonly bool _isNew;

    public BeerEditPage(Beer beer, bool isNew)
    {
        InitializeComponent();
        MarisEntry.HandlerChanged += (_, _) => AllowCommaAndDot(MarisEntry);
        IneseEntry.HandlerChanged += (_, _) => AllowCommaAndDot(IneseEntry);
        _beer = beer;
        _isNew = isNew;

        HeaderLabel.Text = isNew ? "New beer" : "Edit beer";
        TitleEntry.Text = beer.Title;
        CommentEditor.Text = beer.Comment;
        MarisEntry.Text = beer.MarisRating.HasValue ? Beer.Fmt(beer.MarisRating) : "";
        IneseEntry.Text = beer.IneseRating.HasValue ? Beer.Fmt(beer.IneseRating) : "";
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        KeyboardHelper.Hide();
    }

    static void AllowCommaAndDot(Microsoft.Maui.Controls.Entry entry)
    {
#if ANDROID
        if (entry.Handler?.PlatformView is Android.Widget.EditText et)
        {
            et.KeyListener = Android.Text.Method.DigitsKeyListener.GetInstance("0123456789.,");
            // restore the numeric keypad with decimal key (setting a key listener resets the input type)
            et.SetRawInputType(Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal);
        }
#endif
    }

    static object Res(string key) => Application.Current!.Resources[key];

    /// <summary>Empty = valid (not rated). Otherwise 0..10, comma or dot, rounded to 2 decimals.</summary>
    static (bool ok, double? value) ParseRating(string? text)
    {
        text = text?.Trim();
        if (string.IsNullOrEmpty(text)) return (true, null);

        text = text.Replace(',', '.');
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return (false, null);
        if (d < 0 || d > 10) return (false, null);
        return (true, Math.Round(d, 2, MidpointRounding.AwayFromZero));
    }

    void Mark(Border border, bool error)
        => border.Stroke = error ? new SolidColorBrush((Color)Res("Danger")) : Colors.Transparent;

    async void OnSave(object? sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim();
        var (mOk, maris) = ParseRating(MarisEntry.Text);
        var (iOk, inese) = ParseRating(IneseEntry.Text);

        Mark(TitleBorder, string.IsNullOrEmpty(title));
        Mark(MarisBorder, !mOk);
        Mark(IneseBorder, !iOk);

        if (string.IsNullOrEmpty(title))
        {
            ShowError("Please enter a title.");
            return;
        }
        if (!mOk || !iOk)
        {
            ShowError("Ratings must be numbers between 0 and 10 (up to 2 decimals).");
            return;
        }

        _beer.Title = title;
        _beer.Comment = CommentEditor.Text?.Trim() ?? "";
        _beer.MarisRating = maris;
        _beer.IneseRating = inese;

        if (_isNew) Store.Beers.Add(_beer);
        Store.Save();

        KeyboardHelper.Hide();
        await Navigation.PopAsync();
    }

    void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    async void OnBack(object? sender, EventArgs e)
    {
        KeyboardHelper.Hide();
        await Navigation.PopAsync();
    }

    void OnBackgroundTapped(object? sender, TappedEventArgs e)
    {
        TitleEntry.Unfocus();
        CommentEditor.Unfocus();
        MarisEntry.Unfocus();
        IneseEntry.Unfocus();
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
