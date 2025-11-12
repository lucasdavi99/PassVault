using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class NewFolderPage : ContentPage
{
    public NewFolderPage(NewFolderPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}