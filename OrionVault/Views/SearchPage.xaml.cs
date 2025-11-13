using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class SearchPage : ContentPage
{
    public SearchPage(SearchPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}