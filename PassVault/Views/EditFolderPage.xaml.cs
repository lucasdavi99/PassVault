using PassVault.ViewModels;

namespace PassVault.Views;

public partial class EditFolderPage : ContentPage
{
	public EditFolderPage(EditFolderPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}