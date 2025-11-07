using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Interfaces;
using PassVault.Models;
using PassVault.Services;
using PassVault.Services.Security;
using PassVault.Views;

namespace PassVault.ViewModels
{
    public partial class SearchPageViewModel : ObservableObject
    {
        private readonly AccountDatabase _accountDatabase;
        private readonly ILocalizationService _localizationService;
        private readonly IAuthenticationService _authenticationService;

        public bool ShowDesktopDelete { get; } = System.OperatingSystem.IsWindows();

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private ObservableCollection<Account> filteredAccounts = new();

        [ObservableProperty]
        private string pageTitle;

        [ObservableProperty]
        private string pageSubtitle;

        [ObservableProperty]
        private string searchPlaceholder;

        [ObservableProperty]
        private string noResultsTitle;

        [ObservableProperty]
        private string noResultsMessage;

        [ObservableProperty]
        private string initialSearchTitle;

        [ObservableProperty]
        private string initialSearchMessage;

        [ObservableProperty]
        private string tipsTitle;

        [ObservableProperty]
        private string tip1;

        [ObservableProperty]
        private string tip2;

        [ObservableProperty]
        private string tip3;

        [ObservableProperty]
        private string resultsCountFormat;

        [ObservableProperty]
        private string searchForFormat;

        [ObservableProperty]
        private string resultsCountText;

        [ObservableProperty]
        private string searchForText;

        [ObservableProperty]
        private string noResultsForText;

        public IAsyncRelayCommand ExecuteSearchCommand { get; }

        public SearchPageViewModel(AccountDatabase database, ILocalizationService localizationService, IAuthenticationService authenticationService)
        {
            _accountDatabase = database;
            _localizationService = localizationService;
            _authenticationService = authenticationService;
            ExecuteSearchCommand = new AsyncRelayCommand(SearchAsync);
            _localizationService.LanguageChanged += (s, e) => UpdateLocalizedTexts();
            UpdateLocalizedTexts();
            FilteredAccounts.CollectionChanged += (s, e) => UpdateResultsInfo();
        }

        private void UpdateLocalizedTexts()
        {
            PageTitle = L.Text("search.page_title");
            PageSubtitle = L.Text("search.page_subtitle");
            SearchPlaceholder = L.Text("search.placeholder");
            NoResultsTitle = L.Text("search.no_results_title");
            NoResultsMessage = L.Text("search.no_results_message");
            InitialSearchTitle = L.Text("search.initial_search_title");
            InitialSearchMessage = L.Text("search.initial_search_message");
            TipsTitle = L.Text("search.tips_title");
            Tip1 = L.Text("search.tip1");
            Tip2 = L.Text("search.tip2");
            Tip3 = L.Text("search.tip3");
            ResultsCountFormat = L.Text("search.results_count_format");
            SearchForFormat = L.Text("search.search_for_format");
            UpdateResultsInfo();
        }

        private void UpdateResultsInfo()
        {
            ResultsCountText = string.Format(L.Text("search.results_count_format"), FilteredAccounts.Count);
            SearchForText = string.Format(L.Text("search.search_for_format"), SearchText);
            NoResultsForText = string.Format(L.Text("search.no_results_for"), SearchText);
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredAccounts.Clear();
                return;
            }

            var results = await _accountDatabase.SearchAccountsByNameAsync(SearchText);

            FilteredAccounts.Clear();
            foreach (var account in results)
            {
                FilteredAccounts.Add(account);
            }
        }
        [RelayCommand]
        private async Task EditAccount(Account account)
        {
            if (account == null)
                return;

            var parameters = new Dictionary<string, object>
            {
                { "accountId", account.Id },
                { "selectedFields", new Dictionary<string, bool>
                    {
                        { "Username", !string.IsNullOrEmpty(account.Username) },
                        { "Email", !string.IsNullOrEmpty(account.Email) },
                    }
                }
            };

            await Shell.Current.GoToAsync(nameof(EditAccountPage), parameters);
            await SearchAsync();
        }

        [RelayCommand]
        private async Task DeleteAccount(Account account)
        {
            if (account != null)
            {
                bool confirm = await Shell.Current.DisplayAlert("Confirmação", "Deseja realmente excluir este item?", "Sim", "Não");

                if (confirm)
                {
                    if (await EnsureWindowsAuthorizationAsync())
                    {
                        await _accountDatabase.DeleteAccountAsync(account);
                        await SearchAsync();
                        await Shell.Current.DisplayAlert("Sucesso", "Conta excluída com sucesso.", "OK");
                    }
                }
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        partial void OnSearchTextChanged(string value)
        {
            ExecuteSearchCommand.ExecuteAsync(null);
            UpdateResultsInfo();
        }

        private async Task<bool> EnsureWindowsAuthorizationAsync()
        {
            if (!OperatingSystem.IsWindows())
                return true;

            var request = new AppAuthenticationRequest
            {
                Title = L.Text("messages.auth_needed_title"),
                Message = L.Text("messages.auth_needed_message"),
                AllowAlternativeAuthentication = true
            };

            var result = await _authenticationService.AuthenticateAsync(request);

            if (result.Status == AppAuthenticationStatus.NotAvailable)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), L.Text("messages.no_password_configured"), L.Text("common.ok"));
                return false;
            }

            if (!result.IsSuccessful)
            {
                var message = !string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? string.Format(L.Text("messages.auth_error"), result.ErrorMessage)
                    : L.Text("messages.auth_failed");

                await Shell.Current.DisplayAlert(L.Text("common.error"), message, L.Text("common.ok"));
            }

            return result.IsSuccessful;
        }
    }
}
