using System.Globalization;

namespace TodoLists.Converters;

/// <summary>File path -> ImageSource (null when the file doesn't exist).</summary>
public class PathToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || !File.Exists(path)) return null;
        return ImageSource.FromStream(() => File.OpenRead(path));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
