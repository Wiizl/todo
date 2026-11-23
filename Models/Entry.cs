using System.ComponentModel;

namespace TodoLists.Models;

public class ListEntry : INotifyPropertyChanged
{
    bool _done;
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = "";

    public bool Done
    {
        get => _done;
        set
        {
            if (_done == value) return;
            _done = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Done)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
