using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class TutorialPage3 : ContentPage
{
    public TutorialPage3(TutorialPage3ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
