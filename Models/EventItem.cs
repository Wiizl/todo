using System.Globalization;
using System.Text.Json.Serialization;

namespace TodoLists.Models;

public class EventItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public DateTime When { get; set; } = DateTime.Today;   // date + time in one value
    public string Place { get; set; } = "";

    /// <summary>e.g. "Sunday, November 22, 21:59"</summary>
    [JsonIgnore]
    public string WhenText => When.ToString("dddd, MMMM d, HH:mm", CultureInfo.InvariantCulture);
}
