using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Interfaces;
using PassVault.Services;
using PassVault.Services.Billing;

namespace PassVault.ViewModels
{
    public partial class UpgradePageViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;
        private readonly IVipService _vipService;
        private readonly IBillingService _billingService;

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

        public UpgradePageViewModel(ILocalizationService localizationService, IVipService vipService, IBillingService billingService)
        {
            _localizationService = localizationService;
            _vipService = vipService;
            _billingService = billingService;
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
            if (_vipService.IsUserVip())
            {
                await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("upgrade_page.purchase_already_owned"), L.Text("common.ok"));
                return;
            }

            try
            {
                IsBusy = true;
                var result = await _billingService.PurchaseVipAsync();

                await HandleBillingResultAsync(result, isRestore: false);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
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
                var result = await _billingService.RestoreVipAsync();
                await HandleBillingResultAsync(result, isRestore: true);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), ex.Message, L.Text("common.ok"));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task HandleBillingResultAsync(BillingResult result, bool isRestore)
        {
            var successTitle = L.Text("common.success");
            var errorTitle = L.Text("common.error");
            var okText = L.Text("common.ok");

            switch (result.Status)
            {
                case BillingResultStatus.Success:
                    await Shell.Current.DisplayAlert(successTitle,
                        isRestore ? L.Text("upgrade_page.restore_success") : L.Text("upgrade_page.purchase_success"),
                        okText);
                    await Shell.Current.Navigation.PopAsync();
                    break;
                case BillingResultStatus.AlreadyOwned:
                    await Shell.Current.DisplayAlert(successTitle, L.Text("upgrade_page.purchase_already_owned"), okText);
                    await Shell.Current.Navigation.PopAsync();
                    break;
                case BillingResultStatus.Pending:
                    await Shell.Current.DisplayAlert(L.Text("upgrade_page.pending_title"),
                        L.Text("upgrade_page.purchase_pending"),
                        okText);
                    break;
                case BillingResultStatus.UserCancelled:
                    await Shell.Current.DisplayAlert(L.Text("upgrade_page.cancelled_title"),
                        L.Text("upgrade_page.purchase_cancelled"),
                        okText);
                    break;
                case BillingResultStatus.NoPurchasesFound:
                    await Shell.Current.DisplayAlert(L.Text("upgrade_page.restore_title"),
                        L.Text("upgrade_page.restore_none"),
                        okText);
                    break;
                case BillingResultStatus.BillingUnavailable:
                    await Shell.Current.DisplayAlert(errorTitle,
                        L.Text("upgrade_page.billing_unavailable"),
                        okText);
                    break;
                case BillingResultStatus.NetworkError:
                    await Shell.Current.DisplayAlert(errorTitle,
                        L.Text("upgrade_page.network_error"),
                        okText);
                    break;
                case BillingResultStatus.ProductNotFound:
                    await Shell.Current.DisplayAlert(errorTitle,
                        L.Text("upgrade_page.product_not_found"),
                        okText);
                    break;
                case BillingResultStatus.NotSupported:
                    await Shell.Current.DisplayAlert(errorTitle,
                        L.Text("upgrade_page.not_supported"),
                        okText);
                    break;
                default:
                    var message = string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? L.Text("upgrade_page.purchase_error")
                        : result.ErrorMessage;
                    await Shell.Current.DisplayAlert(errorTitle, message, okText);
                    break;
            }
        }
    }
}
