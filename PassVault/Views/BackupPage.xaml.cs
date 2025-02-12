using PassVault.ViewModels;

namespace PassVault.Views;

public partial class BackupPage : ContentPage
{
	public BackupPage (BackupViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}