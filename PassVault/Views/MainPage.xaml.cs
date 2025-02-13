using PassVault.ViewModels;

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

        // Verifica se o alerta de delete já foi exibido
        bool hasSeenDeleteHint = Preferences.Get("HasSeenDeleteHint", false);
        if (!hasSeenDeleteHint)
        {
            await Shell.Current.DisplayAlert("Dica", "Para deletar uma conta ou pasta, toque duas vezes no item.", "OK");
            // Marca como exibido
            Preferences.Set("HasSeenDeleteHint", true);
        }
    }
}