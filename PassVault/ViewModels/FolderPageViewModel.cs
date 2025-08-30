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

        // Propriedade para controlar a visibilidade do estado vazio
        [ObservableProperty]
        private bool isEmpty;

        // Propriedade para controlar a visibilidade da lista de contas
        [ObservableProperty]
        private bool hasAccounts;

        public FolderPageViewModel(AccountDatabase accountDatabase, FolderDatabase folderDatabase)
        {
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
            Accounts = new ObservableCollection<Account>();

            // Registrar para receber mensagens de conta salva
            WeakReferenceMessenger.Default.Register<AccountSavedMessage>(this);

            // Inicializar propriedades de visibilidade
            UpdateVisibilityProperties();
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

                    // Atualizar as propriedades de visibilidade após remoção
                    UpdateVisibilityProperties();

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

                // Atualizar as propriedades de visibilidade após carregar dados
                UpdateVisibilityProperties();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
            }
        }

        // Método para atualizar as propriedades de visibilidade
        private void UpdateVisibilityProperties()
        {
            IsEmpty = Accounts == null || Accounts.Count == 0;
            HasAccounts = !IsEmpty;
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

        // Override da propriedade Accounts para garantir que a visibilidade seja atualizada
        partial void OnAccountsChanged(ObservableCollection<Account> value)
        {
            if (value != null)
            {
                // Registrar para mudanças na coleção
                value.CollectionChanged += (s, e) => UpdateVisibilityProperties();
            }
            UpdateVisibilityProperties();
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