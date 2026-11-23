namespace TodoLists.Controls;

public record DialogAction(string Text, bool Destructive = false);

/// <summary>In-app replacement for DisplayAlert / DisplayPromptAsync.</summary>
public partial class DialogOverlay : ContentView
{
    TaskCompletionSource<object?>? _tcs;
    bool _isPrompt;
    bool _closing;

    public DialogOverlay() => InitializeComponent();

    public bool IsOpen => IsVisible;

    static object Res(string key) => Application.Current!.Resources[key];

    /// <summary>Destructive confirm dialog. Returns true when confirmed.</summary>
    public async Task<bool> ConfirmAsync(string title, string message, string confirmText = "Delete")
    {
        _isPrompt = false;
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        MessageLabel.IsVisible = !string.IsNullOrEmpty(message);
        InputFrame.IsVisible = false;
        BadgeLabel.Text = "🗑";
        Badge.BackgroundColor = (Color)Res("DangerBg");
        ConfirmButton.Text = confirmText;
        ConfirmButton.Background = new SolidColorBrush((Color)Res("Danger"));

        ActionsHost.IsVisible = false;
        ButtonsGrid.IsVisible = true;

        return await ShowAsync() is true;
    }

    /// <summary>Text prompt. Returns the trimmed text, or null when cancelled.</summary>
    public async Task<string?> PromptAsync(string title, string message, string placeholder, string confirmText = "Add")
    {
        _isPrompt = true;
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        MessageLabel.IsVisible = !string.IsNullOrEmpty(message);
        InputFrame.IsVisible = true;
        InputFrame.Stroke = Colors.Transparent;
        Input.Text = "";
        Input.Placeholder = placeholder;
        BadgeLabel.Text = "✨";
        Badge.BackgroundColor = ((Color)Res("Primary")).WithAlpha(0.16f);
        ConfirmButton.Text = confirmText;
        ConfirmButton.Background = (Brush)Res("PrimaryGradient");

        ActionsHost.IsVisible = false;
        ButtonsGrid.IsVisible = true;

        return await ShowAsync() as string;
    }

    public void Cancel() => Close(null);

    async Task<object?> ShowAsync()
    {
        if (_tcs != null) return null;
        _tcs = new TaskCompletionSource<object?>();
        _closing = false;

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

        if (_isPrompt) Input.Focus();
        return await task;
    }

    async void Close(object? result)
    {
        if (_closing || _tcs == null) return;
        _closing = true;

        //Input.Unfocus();
        HideKeyboard();
        await Task.WhenAll(
            Backdrop.FadeToAsync(0, 150),
            Card.FadeToAsync(0, 150),
            Card.ScaleToAsync(0.95, 150));
        IsVisible = false;

        var tcs = _tcs;
        _tcs = null;
        tcs.TrySetResult(result);
    }

    async void OnConfirm(object? sender, EventArgs e)
    {
        if (!_isPrompt) { Close(true); return; }

        var text = Input.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            InputFrame.Stroke = new SolidColorBrush((Color)Res("Danger"));
            foreach (var dx in new double[] { -10, 10, -6, 6, 0 })
                await InputFrame.TranslateToAsync(dx, 0, 45);
            return;
        }
        Close(text);
    }

    public async Task<int> ActionsAsync(string title, string message, string badge,
                                    IList<DialogAction> actions, string cancelText = "Cancel")
    {
        _isPrompt = false;
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        MessageLabel.IsVisible = !string.IsNullOrEmpty(message);
        BadgeLabel.Text = badge;
        Badge.BackgroundColor = ((Color)Res("Primary")).WithAlpha(0.16f);
        InputFrame.IsVisible = false;
        ButtonsGrid.IsVisible = false;
        ActionsHost.IsVisible = true;
        ActionsHost.Children.Clear();

        for (int i = 0; i < actions.Count; i++)
        {
            int index = i;
            var a = actions[i];
            var btn = new Button
            {
                Text = a.Text,
                CornerRadius = 18,
                HeightRequest = 52,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold
            };
            if (a.Destructive) { btn.BackgroundColor = (Color)Res("DangerBg"); btn.TextColor = (Color)Res("Danger"); }
            else { btn.Background = (Brush)Res("PrimaryGradient"); btn.TextColor = Colors.White; }
            btn.Clicked += (_, _) => Close(index);
            ActionsHost.Children.Add(btn);
        }

        var cancel = new Button { Text = cancelText, Style = (Style)Res("SecondaryButton") };
        cancel.Clicked += (_, _) => Close(null);
        ActionsHost.Children.Add(cancel);

        return await ShowAsync() is int chosen ? chosen : -1;
    }

    void HideKeyboard()
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
        Input.Unfocus();
    }

    public async Task AlertAsync(string title, string message)
        => await ActionsAsync(title, message, "⚠️", new List<DialogAction>(), "OK");

    void OnCancel(object? sender, EventArgs e) => Close(null);
    void OnBackdropTapped(object? sender, TappedEventArgs e) => Close(null);
    void OnCardTapped(object? sender, TappedEventArgs e) {
        /* swallow so backdrop doesn't dismiss */
        if (_isPrompt) HideKeyboard();
    }

    void OnInputFocused(object? sender, FocusEventArgs e)
        => InputFrame.Stroke = new SolidColorBrush((Color)Res("Primary"));

    void OnInputUnfocused(object? sender, FocusEventArgs e)
        => InputFrame.Stroke = Colors.Transparent;
}
