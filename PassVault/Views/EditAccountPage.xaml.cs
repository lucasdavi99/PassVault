using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class EditAccountPage : ContentPage
{
    public EditAccountPage(EditAccountPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}