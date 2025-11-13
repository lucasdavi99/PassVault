using OrionVault.ViewModels;

namespace OrionVault.Views;

public partial class TutorialPrivacyPolicyPage : ContentPage
{
    public TutorialPrivacyPolicyPage(TutorialPrivacyPolicyPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
