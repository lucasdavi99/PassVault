using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Models;
using PassVault.Views;
using System.Collections.ObjectModel;


namespace PassVault.ViewModels
{
    public partial class SearchPageViewModel : ObservableObject
    {
        private readonly AccountDatabase _accountDatabase;

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private ObservableCollection<Account> filteredAccounts = new();

        public IAsyncRelayCommand ExecuteSearchCommand { get; }

        public SearchPageViewModel(AccountDatabase database)
        {
            _accountDatabase = database;
            ExecuteSearchCommand = new AsyncRelayCommand(SearchAsync);
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
                    await _accountDatabase.DeleteAccountAsync(account);
                    await SearchAsync();
                    await Shell.Current.DisplayAlert("Sucesso", "Conta excluída com sucesso.", "OK");
                }
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            ExecuteSearchCommand.ExecuteAsync(null);
        }
    }
}
