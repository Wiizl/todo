using System.Globalization;

namespace TodoLists.Converters;

/// <summary>Turns a category name into a stable accent color / soft tint / initial letter.</summary>
public class NameConverter : IValueConverter
{
    static readonly Color[] Palette =
    {
        Color.FromArgb("#6C5CE7"), Color.FromArgb("#0984E3"), Color.FromArgb("#00B894"),
        Color.FromArgb("#E17055"), Color.FromArgb("#E84393"), Color.FromArgb("#F39C12"),
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var name = value as string ?? "";
        int h = 0;
        unchecked { foreach (char ch in name) h = h * 31 + ch; }
        var accent = Palette[Math.Abs(h % Palette.Length)];

        return (parameter as string) switch
        {
            "soft" => accent.WithAlpha(0.16f),
            "initial" => name.Length > 0 ? name.Substring(0, 1).ToUpperInvariant() : "?",
            _ => accent,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
