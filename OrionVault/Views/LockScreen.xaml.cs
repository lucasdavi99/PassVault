using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class LockScreen : ContentPage
{
    public LockScreen(LockScreenViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}