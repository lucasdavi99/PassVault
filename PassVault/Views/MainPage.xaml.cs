using PassVault.ViewModels;
using Microsoft.Maui.Storage;

namespace PassVault.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        bool hasSeenDeleteHint = Preferences.Get("HasSeenDeleteHint", false);
        if (!hasSeenDeleteHint)
        {
            await Shell.Current.DisplayAlert("Dica", "Para deletar uma conta ou pasta, toque duas vezes no item.", "OK");
            Preferences.Set("HasSeenDeleteHint", true);
        }
    }
}