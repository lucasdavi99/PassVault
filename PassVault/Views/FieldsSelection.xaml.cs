using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class FieldsSelection : ContentPage
{
    public FieldsSelection(FieldsSelectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnUsernameToggleTapped(object sender, EventArgs e)
    {
        if (BindingContext is FieldsSelectionViewModel viewModel)
        {
            viewModel.IsUsernameChecked = !viewModel.IsUsernameChecked;
        }
    }

    private void OnEmailToggleTapped(object sender, EventArgs e)
    {
        if (BindingContext is FieldsSelectionViewModel viewModel)
        {
            viewModel.IsEmailChecked = !viewModel.IsEmailChecked;
        }
    }
}