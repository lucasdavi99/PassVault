using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Interfaces;
using PassVault.Services;

namespace PassVault.ViewModels
{
    public partial class UpgradePageViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;
        private readonly IVipService _vipService;

        [ObservableProperty]
        private bool _isBusy;

        // Propriedades de Texto Localizadas
        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string headerTitle;
        [ObservableProperty] private string headerSubtitle;
        [ObservableProperty] private string benefitsTitle;
        [ObservableProperty] private string benefitUnlimitedAccounts;
        [ObservableProperty] private string benefitUnlimitedFolders;
        [ObservableProperty] private string benefitSubfolders;
        [ObservableProperty] private string purchaseButtonText;
        [ObservableProperty] private string restoreButtonText;

        public UpgradePageViewModel(ILocalizationService localizationService, IVipService vipService)
        {
            _localizationService = localizationService;
            _vipService = vipService;
            UpdateLocalizedTexts();
            _localizationService.LanguageChanged += (s, e) => UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = L.Text("upgrade_page.title");
            HeaderTitle = L.Text("upgrade_page.header_title");
            HeaderSubtitle = L.Text("upgrade_page.header_subtitle");
            BenefitsTitle = L.Text("upgrade_page.benefits_title");
            BenefitUnlimitedAccounts = L.Text("upgrade_page.benefit_unlimited_accounts");
            BenefitUnlimitedFolders = L.Text("upgrade_page.benefit_unlimited_folders");
            BenefitSubfolders = L.Text("upgrade_page.benefit_subfolders");
            PurchaseButtonText = L.Text("upgrade_page.purchase_button");
            RestoreButtonText = L.Text("upgrade_page.restore_button");
        }

        [RelayCommand]
        private async Task PurchaseAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                // Lógica de compra será implementada aqui
                await Shell.Current.DisplayAlert("Em Breve", "A funcionalidade de compra será adicionada em breve.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RestoreAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                // Lógica de restauração será implementada aqui
                await Shell.Current.DisplayAlert("Em Breve", "A funcionalidade de restauração será adicionada em breve.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}