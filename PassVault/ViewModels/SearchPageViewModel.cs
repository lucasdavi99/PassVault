using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Models;
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

        partial void OnSearchTextChanged(string value)
        {
            ExecuteSearchCommand.ExecuteAsync(null);
        }
    }
}
