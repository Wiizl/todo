using TodoLists.Models;
using TodoLists.Services;
using TodoLists.Controls;

// >>> If your renamed entry class is not called "Entry", change the name on the right of this alias.
using TodoItem = TodoLists.Models.ListEntry;

namespace TodoLists;

public partial class CategoryPage : ContentPage
{
    readonly Category _category;

    public CategoryPage(Category category)
    {
        InitializeComponent();
        _category = category;
        BindingContext = category;
        List.ItemsSource = category.Entries;
        UpdateCount();
        UpdateIcon();
    }

    void UpdateCount() => CountLabel.Text = _category.Summary;

    protected override bool OnBackButtonPressed()
    {
        if (Dialog.IsOpen) { Dialog.Cancel(); return true; }
        return base.OnBackButtonPressed();
    }

    async void OnBack(object? sender, EventArgs e) => await Navigation.PopAsync();

    async void OnAddEntry(object? sender, EventArgs e)
    {
        var text = await Dialog.PromptAsync("New entry", "What needs to be done?", "Type here…", "Add");
        if (string.IsNullOrWhiteSpace(text)) return;

        _category.Entries.Add(new TodoItem { Text = text });
        Store.Save();
        UpdateCount();
    }

    void OnToggled(object? sender, CheckedChangedEventArgs e)
    {
        Store.Save();
        UpdateCount();
    }

    async void OnDeleteEntry(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not TodoItem entry) return;

        bool ok = await Dialog.ConfirmAsync("Delete entry?", $"“{entry.Text}” will be permanently removed.");
        if (!ok) return;

        _category.Entries.Remove(entry);
        Store.Save();
        UpdateCount();
    }

    void UpdateIcon()
    {
        var path = _category.ImagePath;
        if (path != null && File.Exists(path))
        {
            HeaderImage.Source = ImageSource.FromStream(() => File.OpenRead(path));
            HeaderImage.IsVisible = true;
        }
        else { HeaderImage.Source = null; HeaderImage.IsVisible = false; }
    }

    async void OnIconTapped(object? sender, TappedEventArgs e)
    {
        var actions = new List<DialogAction> { new("Choose from gallery") };
        if (!string.IsNullOrEmpty(_category.ImageFile))
            actions.Add(new DialogAction("Remove image", Destructive: true));

        int choice = await Dialog.ActionsAsync("Category icon",
            "Pick a picture to show instead of the letter.", "🖼️", actions);

        if (choice == 0) await PickImageAsync();
        else if (choice == 1) RemoveImage();
    }

    async Task PickImageAsync()
    {
        try
        {
            var file = await GalleryPicker.PickImageAsync();
            if (file == null) return;

            var newName = await ImageService.SaveCategoryImageAsync(file, _category.Id);
            var oldName = _category.ImageFile;
            _category.ImageFile = newName;
            Store.Save();
            ImageService.Delete(oldName);
            UpdateIcon();
        }
        catch (Exception)
        {
            await Dialog.AlertAsync("Couldn’t use that image", "Please try a different picture.");
        }
    }

    void RemoveImage()
    {
        var old = _category.ImageFile;
        _category.ImageFile = null;
        Store.Save();
        ImageService.Delete(old);
        UpdateIcon();
    }
}
