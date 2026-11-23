using TodoLists.Models;
using TodoLists.Services;

namespace TodoLists;

public partial class EventsPage : ContentPage
{
    public EventsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Refresh();
    }

    protected override bool OnBackButtonPressed()
    {
        if (Dialog.IsOpen) { Dialog.Cancel(); return true; }
        return base.OnBackButtonPressed();
    }

    void Refresh()
    {
        // shown in chronological order
        EventList.ItemsSource = null;
        EventList.ItemsSource = Store.Events.OrderBy(e => e.When).ToList();
        int n = Store.Events.Count;
        SubtitleLabel.Text = n == 0 ? "Nothing planned yet" : n == 1 ? "1 event" : $"{n} events";
    }

    async void OnAddEvent(object? sender, EventArgs e)
        => await Navigation.PushAsync(new EventEditPage(new EventItem(), isNew: true));

    async void OnEventClicked(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is EventItem ev)
            await Navigation.PushAsync(new EventEditPage(ev, isNew: false));
    }

    async void OnDeleteEvent(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not EventItem ev) return;

        bool ok = await Dialog.ConfirmAsync("Delete event?",
            $"“{ev.Title}” will be permanently removed.");
        if (!ok) return;

        Store.Events.Remove(ev);
        Store.Save();
        Refresh();
    }
}
