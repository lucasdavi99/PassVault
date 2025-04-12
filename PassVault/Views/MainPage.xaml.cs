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

    private double totalX = 0;
    private double totalY = 0;

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                totalX = 0;
                totalY = 0;
                break;
            case GestureStatus.Running:
                totalX += e.TotalX;
                totalY += e.TotalY;
                break;
            case GestureStatus.Completed:
                if (Math.Abs(totalX) > Math.Abs(totalY) && Math.Abs(totalX) > 50)
                {
                    var viewModel = BindingContext as MainPageViewModel;
                    if (totalX > 0) // Deslize para a direita
                    {
                        viewModel?.SwipeRightCommand?.Execute(null);
                    }
                    else // Deslize para a esquerda
                    {
                        viewModel?.SwipeLeftCommand?.Execute(null);
                    }
                }
                else
                {
                    Console.WriteLine("Gesture too small or vertical");
                }
                break;
        }
    }
}