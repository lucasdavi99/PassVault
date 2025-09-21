using PassVault.ViewModels;

namespace PassVault.Views;

public partial class TutorialPage2 : ContentPage
{
    public TutorialPage2(TutorialPage2ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}