using TodoLists.Models;
using TodoLists.Services;

namespace TodoLists;

public partial class KaraokePage : ContentPage
{
    public KaraokePage()
    {
        InitializeComponent();
        KaraokeItem.DoneChanged += OnDoneChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NormalizeOrder();
        List.ItemsSource = null;
        List.ItemsSource = Store.Karaoke;
        UpdateCount();
    }

    protected override bool OnBackButtonPressed()
    {
        if (Dialog.IsOpen) { Dialog.Cancel(); return true; }
        return base.OnBackButtonPressed();
    }

    static int UndoneCount() => Store.Karaoke.Count(k => !k.Done);

    /// <summary>Make sure all done items are after all open ones (keeps existing order inside each group).</summary>
    static void NormalizeOrder()
    {
        var ordered = Store.Karaoke.Where(k => !k.Done).Concat(Store.Karaoke.Where(k => k.Done)).ToList();
        bool changed = false;
        for (int i = 0; i < ordered.Count; i++)
        {
            int cur = Store.Karaoke.IndexOf(ordered[i]);
            if (cur != i) { Store.Karaoke.Move(cur, i); changed = true; }
        }
        if (changed) Store.Save();
    }

    void UpdateCount()
    {
        int n = Store.Karaoke.Count, done = n - UndoneCount();
        SubtitleLabel.Text = n == 0 ? "Nothing yet" : $"{n} item(s) · {done} done";
    }

    /// <summary>Ticked item goes to the very bottom; un-ticked item goes back to the end of the open items.</summary>
    void OnDoneChanged(KaraokeItem item)
    {
        Dispatcher.Dispatch(() =>
        {
            int from = Store.Karaoke.IndexOf(item);
            if (from >= 0)
            {
                int to = item.Done ? Store.Karaoke.Count - 1 : UndoneCount() - 1;
                if (to != from) Store.Karaoke.Move(from, to);
            }
            Store.Save();
            UpdateCount();
        });
    }

    async void OnAdd(object? sender, EventArgs e)
    {
        var text = await Dialog.PromptAsync("New entry", "What do you want to sing?", "Type here…", "Add");
        if (string.IsNullOrWhiteSpace(text)) return;

        // new entries go above the done ones
        Store.Karaoke.Insert(UndoneCount(), new KaraokeItem { Text = text });
        Store.Save();
        UpdateCount();
    }

    async void OnDelete(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not KaraokeItem item) return;

        bool ok = await Dialog.ConfirmAsync("Delete entry?", $"“{item.Text}” will be permanently removed.");
        if (!ok) return;

        Store.Karaoke.Remove(item);
        Store.Save();
        UpdateCount();
    }
}
