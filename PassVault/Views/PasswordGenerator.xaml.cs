using PassVault.ViewModels;

namespace PassVault.Views;

public partial class PasswordGenerator : ContentPage
{
	public PasswordGenerator(PasswordGeneratorViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}