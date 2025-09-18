using PassVault.ViewModels;

namespace PassVault.Views;

public partial class LockScreen : ContentPage
{
    public LockScreen(LockScreenViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}