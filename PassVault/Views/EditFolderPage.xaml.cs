using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class EditFolderPage : ContentPage
{
    public EditFolderPage(EditFolderPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}