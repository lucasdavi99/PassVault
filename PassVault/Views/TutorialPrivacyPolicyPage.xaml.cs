using PassVault.ViewModels;

namespace PassVault.Views;

public partial class TutorialPrivacyPolicyPage : ContentPage
{
    public TutorialPrivacyPolicyPage(TutorialPrivacyPolicyPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
