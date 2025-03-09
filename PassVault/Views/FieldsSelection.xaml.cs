using PassVault.ViewModels;

namespace PassVault.Views;

public partial class FieldsSelection : ContentPage
{
	public FieldsSelection(FieldsSelectionViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}