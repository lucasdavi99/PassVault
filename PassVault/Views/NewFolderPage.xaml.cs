using PassVault.ViewModels;

namespace PassVault.Views;

public partial class NewFolderPage : ContentPage
{
	public NewFolderPage(NewAccountPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}