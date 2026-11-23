using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace TodoLists;

/// <summary>Bottom tab bar, each tab with its own navigation stack.</summary>
public class MainTabs : Microsoft.Maui.Controls.TabbedPage
{
    NavigationPage? _previous;

    public MainTabs()
    {
        Children.Add(Wrap(new CategoriesPage(), "My Lists", "📝"));
        Children.Add(Wrap(new BeerPage(), "Beer", "🍺"));
        Children.Add(Wrap(new KaraokePage(), "Karaoke", "🎤"));
        Children.Add(Wrap(new EventsPage(), "Events", "🎉"));

        // no page-by-page scrolling through the tabs in between
        this.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetIsSmoothScrollEnabled(false);

        // follow the light/dark palette defined in App.xaml.cs
        SetDynamicResource(BarBackgroundColorProperty, "CardBg");
        SetDynamicResource(SelectedTabColorProperty, "Primary");
        SetDynamicResource(UnselectedTabColorProperty, "TextMuted");

        _previous = CurrentPage as NavigationPage;
        CurrentPageChanged += OnTabChanged;
    }

    async void OnTabChanged(object? sender, EventArgs e)
    {
        var left = _previous;
        var now = CurrentPage as NavigationPage;
        _previous = now;
        if (left == null || now == null || left == now) return;

        // leaving a tab while its Beer/Event edit form is open returns that tab to its list
        if (left.CurrentPage is BeerEditPage or EventEditPage)
            await left.PopToRootAsync(false);

        // short slide-in of the new tab from the side it sits on
        int dir = Children.IndexOf(now) > Children.IndexOf(left) ? 1 : -1;
        try
        {
            now.Opacity = 0;
            now.TranslationX = dir * 80;
            await Task.WhenAll(now.TranslateToAsync(0, 0, 250, Easing.CubicOut), now.FadeToAsync(1, 250));
        }
        finally
        {
            now.TranslationX = 0;
            now.Opacity = 1;
        }
    }

    static NavigationPage Wrap(Page root, string title, string glyph) => new(root)
    {
        Title = title,
        IconImageSource = new FontImageSource { Glyph = glyph, Size = 22 },
    };
}
