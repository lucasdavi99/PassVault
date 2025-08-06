using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Views;

namespace PassVault.ViewModels
{
    public partial class FolderPageViewModel : ObservableObject, IQueryAttributable, IRecipient<AccountSavedMessage>, IDisposable
    {
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;

        [ObservableProperty]
        private int folderId;

        [ObservableProperty]
        private Folder folder;

        [ObservableProperty]
        private ObservableCollection<Account> accounts;

        public FolderPageViewModel(AccountDatabase accountDatabase, FolderDatabase folderDatabase)
        {
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
            Accounts = new ObservableCollection<Account>();

            // Registrar para receber mensagens de conta salva
            WeakReferenceMessenger.Default.Register<AccountSavedMessage>(this);
        }

        [RelayCommand]
        public async Task AddNewItemAsync()
        {
            await Shell.Current.GoToAsync($"{nameof(FieldsSelection)}?folderId={FolderId}", true);
        }

        [RelayCommand]
        private async Task EditFolder(Folder folder) => await Shell.Current.GoToAsync($"{nameof(EditFolderPage)}?folderId={folder.Id}");

        [RelayCommand]
        public async Task EditAccountInFolder(Account account)
        {
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
        }

        [RelayCommand]
        public async Task DeleteAccountInFolder(Account account)
        {
            if (account != null)
            {
                bool confirm = await Shell.Current.DisplayAlert("Confirmação", "Deseja realmente excluir este item?", "Sim", "Não");

                if (confirm)
                {
                    await _accountDatabase.DeleteAccountAsync(account);

                    // Remover da coleção local imediatamente
                    Accounts.Remove(account);

                    await Shell.Current.DisplayAlert("Sucesso", "Conta excluída com sucesso.", "OK");
                }
            }
        }

        [RelayCommand]
        public async Task GoToHome()
        {
            await Shell.Current.GoToAsync("///MainPage");
        }

        public async Task LoadDataAsync()
        {
            try
            {
                Folder = await _folderDatabase.GetFolderAsync(FolderId);
                var items = await _accountDatabase.GetAccountsByFolderIdAsync(FolderId);
                var sortedItems = items
                    .OrderBy(account => account.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                Accounts.Clear();
                foreach (var item in sortedItems)
                {
                    Accounts.Add(item);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            }
        }

        // Handler para quando uma conta é salva
        public async void Receive(AccountSavedMessage message)
        {
            if (message.Value)
            {
                // Recarregar dados quando uma conta for salva
                await LoadDataAsync();
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("folderId") && int.TryParse(query["folderId"]?.ToString(), out int folderId))
            {
                FolderId = folderId;
                await LoadDataAsync();
            }
        }

        // IDisposable implementation
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                WeakReferenceMessenger.Default.UnregisterAll(this);
                Accounts?.Clear();
                _disposed = true;
            }
        }

        ~FolderPageViewModel()
        {
            Dispose(false);
        }
    }
}