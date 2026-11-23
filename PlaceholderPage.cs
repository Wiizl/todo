namespace TodoLists;

/// <summary>Empty "coming soon" tab page. Replace with the real page later.</summary>
public class PlaceholderPage : ContentPage
{
    public PlaceholderPage(string title, string glyph, string hint = "Coming soon")
    {
        NavigationPage.SetHasNavigationBar(this, false);
        SetDynamicResource(BackgroundColorProperty, "PageBg");

        var heading = new Label { Text = title, FontSize = 36, FontAttributes = FontAttributes.Bold, Margin = new Thickness(4, 20, 0, 10) };
        heading.SetDynamicResource(Label.TextColorProperty, "TextMain");

        var icon = new Label { Text = glyph, FontSize = 56, HorizontalOptions = LayoutOptions.Center };
        var name = new Label { Text = title, FontSize = 20, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
        name.SetDynamicResource(Label.TextColorProperty, "TextMain");
        var sub = new Label { Text = hint, HorizontalOptions = LayoutOptions.Center };
        sub.SetDynamicResource(Label.TextColorProperty, "TextMuted");

        var center = new VerticalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center, Children = { icon, name, sub } };

        var grid = new Grid { Padding = new Thickness(20, 12), RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        grid.Add(heading, 0, 0);
        grid.Add(center, 0, 1);
        Content = grid;
    }
}
