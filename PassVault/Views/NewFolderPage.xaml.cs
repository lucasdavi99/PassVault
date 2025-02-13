using PassVault.ViewModels;

namespace PassVault.Views;

public partial class NewFolderPage : ContentPage
{
	public NewFolderPage(NewFolderPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}