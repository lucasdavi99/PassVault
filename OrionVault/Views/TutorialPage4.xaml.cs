using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class TutorialPage4 : ContentPage
{
    public TutorialPage4(TutorialPage4ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}