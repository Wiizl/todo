using System.Globalization;
using System.Text.Json.Serialization;

namespace TodoLists.Models;

public class Beer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Comment { get; set; } = "";
    public double? MarisRating { get; set; }   // 0.00 - 10.00, null = not rated yet
    public double? IneseRating { get; set; }

    [JsonIgnore]
    public string RatingsText => $"Māris {Fmt(MarisRating)}  ·  Inese {Fmt(IneseRating)}";

    public static string Fmt(double? rating)
        => rating.HasValue ? rating.Value.ToString("0.00", CultureInfo.InvariantCulture) : "–";
}
