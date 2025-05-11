using PassVault.ViewModels;

namespace PassVault.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnCarouselItemChanged(object sender, CurrentItemChangedEventArgs e)
    {
        if (e.CurrentItem is string tabName)
        {
            var viewModel = BindingContext as MainPageViewModel;
            viewModel?.SelectTabCommand?.Execute(tabName);
        }
    }
}