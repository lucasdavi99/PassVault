using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class TutorialPage1 : ContentPage
{
    public TutorialPage1(TutorialPage1ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}