using PassVault.ViewModels;

namespace PassVault.Views;

public partial class NewAccountPage : ContentPage
{
	public NewAccountPage(NewAccountPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}