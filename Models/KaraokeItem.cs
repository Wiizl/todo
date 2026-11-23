using System.ComponentModel;

namespace TodoLists.Models;

public class KaraokeItem : INotifyPropertyChanged
{
    bool _done;
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = "";

    /// <summary>Raised only when Done really changes (not when a recycled row is re-bound).</summary>
    public static event Action<KaraokeItem>? DoneChanged;

    public bool Done
    {
        get => _done;
        set
        {
            if (_done == value) return;
            _done = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Done)));
            DoneChanged?.Invoke(this);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
