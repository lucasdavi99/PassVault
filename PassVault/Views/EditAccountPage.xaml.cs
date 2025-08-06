using PassVault.ViewModels;

namespace PassVault.Views;

public partial class EditAccountPage : ContentPage
{
    public EditAccountPage(EditAccountPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}