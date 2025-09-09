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
    public partial class FolderPageViewModel : ObservableObject, IQueryAttributable, IRecipient<AccountSavedMessage>, IRecipient<FolderSavedMessage>, IDisposable
    {
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;

        [ObservableProperty]
        private int folderId;

        [ObservableProperty]
        private Folder folder;

        [ObservableProperty]
        private ObservableCollection<Account> accounts;

        [ObservableProperty]
        private ObservableCollection<Folder> subFolders;

        // Propriedades para controlar a visibilidade
        [ObservableProperty]
        private bool isEmpty;

        [ObservableProperty]
        private bool hasAccounts;

        [ObservableProperty]
        private bool hasSubFolders;

        // Controle das abas
        [ObservableProperty]
        private bool isAccountsTabActive = true;

        [ObservableProperty]
        private bool isFoldersTabActive = false;

        public FolderPageViewModel(AccountDatabase accountDatabase, FolderDatabase folderDatabase)
        {
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
            Accounts = new ObservableCollection<Account>();
            SubFolders = new ObservableCollection<Folder>();

            // Registrar para receber mensagens
            WeakReferenceMessenger.Default.Register<AccountSavedMessage>(this);
            WeakReferenceMessenger.Default.Register<FolderSavedMessage>(this);

            // Inicializar propriedades de visibilidade
            UpdateVisibilityProperties();
        }

        [RelayCommand]
        public async Task AddNewItemAsync()
        {
            await Shell.Current.GoToAsync($"{nameof(FieldsSelection)}?folderId={FolderId}", true);
        }

        [RelayCommand]
        public async Task AddNewSubFolderAsync()
        {
            await Shell.Current.GoToAsync($"{nameof(NewFolderPage)}?parentFolderId={FolderId}", true);
        }

        [RelayCommand]
        private async Task EditFolder(Folder folder) => await Shell.Current.GoToAsync($"{nameof(EditFolderPage)}?folderId={folder.Id}");

        [RelayCommand]
        private async Task OpenSubFolder(Folder subFolder)
        {
            if (subFolder == null) return;

            try
            {
                await Shell.Current.GoToAsync($"{nameof(FolderPage)}?folderId={subFolder.Id}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao abrir pasta: {ex.Message}", "OK");
            }
        }

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
                    Accounts.Remove(account);
                    await Shell.Current.DisplayAlert("Sucesso", "Item excluído com sucesso", "OK");
                    UpdateVisibilityProperties();
                }
            }
        }

        [RelayCommand]
        public async Task DeleteSubFolder(Folder subFolder)
        {
            if (subFolder == null) return;

            try
            {
                bool confirm = await Shell.Current.DisplayAlert("Confirmação", "Deseja realmente excluir essa pasta? Todos os itens e subpastas dentro serão excluídos", "Sim", "Não");

                if (confirm)
                {
                    await _folderDatabase.DeleteFolderAsync(subFolder);
                    SubFolders.Remove(subFolder);
                    await Shell.Current.DisplayAlert("Sucesso", "Pasta excluída com sucesso.", "OK");
                    UpdateVisibilityProperties();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao excluir pasta: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task GoToHome()
        {
            await Shell.Current.GoToAsync("///MainPage");
        }

        [RelayCommand]
        public void ShowAccountsTab()
        {
            IsAccountsTabActive = true;
            IsFoldersTabActive = false;
        }

        [RelayCommand]
        public void ShowFoldersTab()
        {
            IsAccountsTabActive = false;
            IsFoldersTabActive = true;
        }

        public async Task LoadDataAsync()
        {
            try
            {
                Folder = await _folderDatabase.GetFolderAsync(FolderId);

                // Carregar contas da pasta
                var items = await _accountDatabase.GetAccountsByFolderIdAsync(FolderId);
                var sortedItems = items
                    .OrderBy(account => account.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                Accounts.Clear();
                foreach (var item in sortedItems)
                {
                    Accounts.Add(item);
                }

                // Carregar subpastas
                var subFolders = await _folderDatabase.GetSubFoldersAsync(FolderId);
                SubFolders.Clear();
                foreach (var subFolder in subFolders)
                {
                    SubFolders.Add(subFolder);
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
            HasAccounts = Accounts != null && Accounts.Count > 0;
            HasSubFolders = SubFolders != null && SubFolders.Count > 0;
            IsEmpty = !HasAccounts && !HasSubFolders;
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

        // Handler para quando uma pasta é salva
        public async void Receive(FolderSavedMessage message)
        {
            if (message.Value)
            {
                // Recarregar dados quando uma pasta for salva
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

        // Override das propriedades para garantir que a visibilidade seja atualizada
        partial void OnAccountsChanged(ObservableCollection<Account> value)
        {
            if (value != null)
            {
                value.CollectionChanged += (s, e) => UpdateVisibilityProperties();
            }
            UpdateVisibilityProperties();
        }

        partial void OnSubFoldersChanged(ObservableCollection<Folder> value)
        {
            if (value != null)
            {
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
                SubFolders?.Clear();
                _disposed = true;
            }
        }

        ~FolderPageViewModel()
        {
            Dispose(false);
        }
    }
}