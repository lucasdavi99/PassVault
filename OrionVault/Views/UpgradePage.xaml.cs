using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class UpgradePage : ContentPage
{
    public UpgradePage(UpgradePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}