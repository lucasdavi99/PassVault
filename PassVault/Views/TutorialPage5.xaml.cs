using PassVault.ViewModels;

namespace PassVault.Views;

public partial class TutorialPage5 : ContentPage
{
    public TutorialPage5(TutorialPage5ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}