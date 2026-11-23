using TodoLists.Models;
using TodoLists.Services;

namespace TodoLists;

public partial class BeerPage : ContentPage
{
    public BeerPage()
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
        BeerList.ItemsSource = null;
        BeerList.ItemsSource = Store.Beers;
        int n = Store.Beers.Count;
        SubtitleLabel.Text = n == 0 ? "Nothing rated yet" : n == 1 ? "1 beer" : $"{n} beers";
    }

    async void OnAddBeer(object? sender, EventArgs e)
        => await Navigation.PushAsync(new BeerEditPage(new Beer(), isNew: true));

    async void OnBeerClicked(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Beer b)
            await Navigation.PushAsync(new BeerEditPage(b, isNew: false));
    }

    async void OnDeleteBeer(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Beer b) return;

        bool ok = await Dialog.ConfirmAsync("Delete beer?",
            $"“{b.Title}” and its ratings will be permanently removed.");
        if (!ok) return;

        Store.Beers.Remove(b);
        Store.Save();
        Refresh();
    }
}
