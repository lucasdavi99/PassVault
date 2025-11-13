using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class BackupPage : ContentPage
{
    public BackupPage(BackupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}