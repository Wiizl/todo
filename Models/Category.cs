using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace TodoLists.Models;

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string? ImageFile { get; set; }
    public ObservableCollection<ListEntry> Entries { get; set; } = new();

    [JsonIgnore]
    public string Summary
    {
        get
        {
            int done = Entries.Count(e => e.Done);
            return Entries.Count == 0 ? "Empty" : $"{Entries.Count} item(s) · {done} done";
        }
    }

    [JsonIgnore]
    public string? ImagePath => string.IsNullOrEmpty(ImageFile)
    ? null : Path.Combine(FileSystem.AppDataDirectory, ImageFile);

    [JsonIgnore]
    public bool HasImage => ImagePath is { } p && File.Exists(p);
}
