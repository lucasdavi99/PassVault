using PassVault.ViewModels;

namespace PassVault.Views;

public partial class UpgradePage : ContentPage
{
    public UpgradePage(UpgradePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}