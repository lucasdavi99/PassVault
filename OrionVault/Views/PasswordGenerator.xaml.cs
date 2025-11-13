using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class PasswordGenerator : ContentPage
{
    public PasswordGenerator(PasswordGeneratorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}