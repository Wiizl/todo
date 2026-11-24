using TodoLists.Models;
using TodoLists.Services;

namespace TodoLists;

public partial class CategoriesPage : ContentPage
{
    List<Category> _view = new();

    public CategoriesPage()
    {
        InitializeComponent();
        List.HandlerChanged += (_, _) => DragReorder.Attach(List,
    (from, to) =>
    {
        var item = _view[from];
        _view.RemoveAt(from);
        _view.Insert(to, item);
        Store.Categories.Move(from, to);
    },
    () => Store.Save(),
    index => _view[index].IsFavorite);
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
        //List.ItemsSource = null;
        //List.ItemsSource = Store.Categories;
        NormalizeFavorites();
        _view = Store.Categories.ToList();
        List.ItemsSource = null;
        List.ItemsSource = _view;

        int n = Store.Categories.Count;
        SubtitleLabel.Text = n == 0 ? "Create a category to get started"
                                    : n == 1 ? "1 category" : $"{n} categories";
    }

    async void OnAddCategory(object? sender, EventArgs e)
    {
        var name = await Dialog.PromptAsync("New category", "Give your list a name", "e.g. Groceries", "Create");
        if (string.IsNullOrWhiteSpace(name)) return;

        Store.Categories.Add(new Category { Name = name });
        Store.Save();
        Refresh();
    }

    async void OnCategoryClicked(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Category c)
            await Navigation.PushAsync(new CategoryPage(c));
    }

    async void OnDeleteCategory(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Category c) return;
        if (c.IsFavorite) return;

        bool ok = await Dialog.ConfirmAsync("Delete category?",
            $"“{c.Name}” and all of its entries will be permanently removed.");
        if (!ok) return;

        Store.Categories.Remove(c);
        Store.Save();
        ImageService.Delete(c.ImageFile);
        Refresh();
    }

    static void NormalizeFavorites()
    {
        var ordered = Store.Categories.Where(c => c.IsFavorite)
            .Concat(Store.Categories.Where(c => !c.IsFavorite)).ToList();
        bool changed = false;
        for (int i = 0; i < ordered.Count; i++)
        {
            int cur = Store.Categories.IndexOf(ordered[i]);
            if (cur != i) { Store.Categories.Move(cur, i); changed = true; }
        }
        if (changed) Store.Save();
    }

    void OnToggleFavorite(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Category c) return;
        c.IsFavorite = !c.IsFavorite;

        int favs = Store.Categories.Count(x => x.IsFavorite);
        int from = Store.Categories.IndexOf(c);
        int to = c.IsFavorite ? favs - 1 : favs;   // end of the favourites / start of the rest
        if (to != from) Store.Categories.Move(from, to);

        Store.Save();
        Refresh();
    }
}
