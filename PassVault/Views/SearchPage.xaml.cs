using PassVault.ViewModels;

namespace PassVault.Views;

public partial class SearchPage : ContentPage
{
    public SearchPage(SearchPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}