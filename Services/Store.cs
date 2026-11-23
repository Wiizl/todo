using System.Collections.ObjectModel;
using System.Text.Json;
using TodoLists.Models;

namespace TodoLists.Services;

/// <summary>Root object of data.json.</summary>
public class AppData
{
    public List<Category> Categories { get; set; } = new();
    public List<Beer> Beers { get; set; } = new();
    public List<EventItem> Events { get; set; } = new();
    public List<KaraokeItem> Karaoke { get; set; } = new();
}

public static class Store
{
    static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "data.json");
    static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static ObservableCollection<Category> Categories { get; private set; } = new();
    public static ObservableCollection<Beer> Beers { get; private set; } = new();
    public static ObservableCollection<EventItem> Events { get; private set; } = new();
    public static ObservableCollection<KaraokeItem> Karaoke { get; private set; } = new();

    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var json = File.ReadAllText(FilePath);

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                // legacy format: the file was just a list of categories
                var list = JsonSerializer.Deserialize<List<Category>>(json);
                if (list != null) Categories = new ObservableCollection<Category>(list);
            }
            else
            {
                var data = JsonSerializer.Deserialize<AppData>(json);
                if (data != null)
                {
                    Categories = new ObservableCollection<Category>(data.Categories ?? new());
                    Beers = new ObservableCollection<Beer>(data.Beers ?? new());
                    Events = new ObservableCollection<EventItem>(data.Events ?? new());
                    Karaoke = new ObservableCollection<KaraokeItem>(data.Karaoke ?? new());
                }
            }
        }
        catch { /* corrupt file -> start empty */ }
    }

    public static void Save()
    {
        try
        {
            var data = new AppData
            {
                Categories = Categories.ToList(),
                Beers = Beers.ToList(),
                Events = Events.ToList(),
                Karaoke = Karaoke.ToList(),
            };
            File.WriteAllText(FilePath, JsonSerializer.Serialize(data, Options));
        }
        catch { }
    }
}
