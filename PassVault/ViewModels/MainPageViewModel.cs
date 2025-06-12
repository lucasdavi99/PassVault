using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Services;
using PassVault.Views;

namespace PassVault.ViewModels
{
    public partial class MainPageViewModel : ObservableObject, IRecipient<AccountSavedMessage>, IRecipient<FolderSavedMessage>, IDisposable
    {
        private readonly AccountDatabase _database;
        private readonly FolderDatabase _folderDatabase;
        private readonly CacheService _cacheService;

        // Paginação
        private int _currentAccountPage = 0;
        private int _currentFolderPage = 0;
        private const int PageSize = 20;
        private bool _isLoadingAccounts = false;
        private bool _isLoadingFolders = false;
        private bool _hasMoreAccounts = true;
        private bool _hasMoreFolders = true;

        [ObservableProperty]
        private string _selectedTab = "Itens";

        [ObservableProperty]
        private int _tabPosition = 0;

        [ObservableProperty]
        private string _selectedAction = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Account> _accounts = new();

        [ObservableProperty]
        private ObservableCollection<Folder> _folders = new();

        [ObservableProperty]
        private bool _isRefreshing = false;

        public IRelayCommand SelectTabCommand { get; }
        public IAsyncRelayCommand LoadMoreAccountsCommand { get; }
        public IAsyncRelayCommand LoadMoreFoldersCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }

        public MainPageViewModel(AccountDatabase database, FolderDatabase folderDatabase, CacheService cacheService)
        {
            _database = database;
            _folderDatabase = folderDatabase;
            _cacheService = cacheService;

            SelectTabCommand = new AsyncRelayCommand<string>(OnTabSelected);
            LoadMoreAccountsCommand = new AsyncRelayCommand(LoadMoreAccountsAsync);
            LoadMoreFoldersCommand = new AsyncRelayCommand(LoadMoreFoldersAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshCurrentTabAsync);

            SelectedTab = "Itens";
            TabPosition = 0;

            WeakReferenceMessenger.Default.Register<AccountSavedMessage>(this);
            WeakReferenceMessenger.Default.Register<FolderSavedMessage>(this);

            // Carregamento inicial
            _ = Task.Run(async () =>
            {
                await LoadAccountsAsync(refresh: true);
                await LoadFoldersAsync(refresh: true);
            });
        }

        partial void OnTabPositionChanged(int value)
        {
            SelectedTab = value == 0 ? "Itens" : "Pastas";
            _ = RefreshCurrentTabAsync();
        }

        private async Task RefreshCurrentTabAsync()
        {
            if (IsRefreshing) return;

            IsRefreshing = true;
            try
            {
                if (SelectedTab == "Itens")
                    await LoadAccountsAsync(refresh: true);
                else if (SelectedTab == "Pastas")
                    await LoadFoldersAsync(refresh: true);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task OnTabSelected(string? tab)
        {
            if (string.IsNullOrEmpty(tab)) return;

            await Task.Delay(100); // Suavizar transição
            SelectedTab = tab;
            TabPosition = tab == "Itens" ? 0 : 1;
            await RefreshCurrentTabAsync();
        }

        [RelayCommand]
        private async Task SelectAction(string action)
        {
            try
            {
                switch (action)
                {
                    case "Export/Import":
                        await Shell.Current.GoToAsync(nameof(BackupPage));
                        break;

                    case "Add":
                        if (SelectedTab == "Itens")
                        {
                            await Shell.Current.GoToAsync(nameof(FieldsSelection));
                        }
                        else if (SelectedTab == "Pastas")
                        {
                            await Shell.Current.GoToAsync(nameof(NewFolderPage));
                        }
                        break;

                    case "Search":
                        await Shell.Current.GoToAsync(nameof(SearchPage));
                        break;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task EditAccount(Account account)
        {
            if (account == null) return;

            try
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
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao editar conta: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteAccount(Account account)
        {
            if (account == null) return;

            try
            {
                bool confirm = await Shell.Current.DisplayAlert("Confirmação", "Deseja realmente excluir este item?", "Sim", "Não");

                if (confirm)
                {
                    await _database.DeleteAccountAsync(account);

                    // Remover da coleção local
                    Accounts.Remove(account);

                    // Limpar cache
                    _cacheService.ClearAccountsCache();

                    await Shell.Current.DisplayAlert("Sucesso", "Conta excluída com sucesso.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao excluir conta: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteFolder(Folder folder)
        {
            if (folder == null) return;

            try
            {
                bool confirm = await Shell.Current.DisplayAlert("Confirmação", "Deseja realmente excluir essa pasta? Todos os itens dentro da pasta serão excluidos", "Sim", "Não");

                if (confirm)
                {
                    await _folderDatabase.DeleteFolderAsync(folder);

                    // Remover da coleção local
                    Folders.Remove(folder);

                    // Limpar cache
                    _cacheService.ClearFoldersCache();
                    _cacheService.ClearAccountsCache(); // Contas também podem ter sido afetadas

                    await Shell.Current.DisplayAlert("Sucesso", "Pasta excluída com sucesso.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao excluir pasta: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task OpenFolderAsync(Folder folder)
        {
            if (folder == null) return;

            try
            {
                await Shell.Current.GoToAsync($"{nameof(FolderPage)}?folderId={folder.Id}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao abrir pasta: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Help() =>
            await Shell.Current.DisplayAlert("Ajuda", "Para deletar uma conta ou pasta, arraste para o lado esquerdo.", "OK");

        // Métodos de carregamento otimizados
        private async Task LoadAccountsAsync(bool refresh = false)
        {
            if (_isLoadingAccounts && !refresh) return;

            _isLoadingAccounts = true;
            try
            {
                if (refresh)
                {
                    _currentAccountPage = 0;
                    _hasMoreAccounts = true;

                    await MainThread.InvokeOnMainThreadAsync(() => Accounts.Clear());
                }

                if (!_hasMoreAccounts) return;

                var accounts = await _cacheService.GetOrSetAsync(
                    $"accounts_page_{_currentAccountPage}",
                    () => _database.GetAccountsWithoutFolderAsync(_currentAccountPage * PageSize, PageSize),
                    TimeSpan.FromMinutes(5));

                if (accounts.Count < PageSize)
                    _hasMoreAccounts = false;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var account in accounts)
                    {
                        Accounts.Add(account);
                    }
                });

                _currentAccountPage++;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar contas: {ex.Message}", "OK");
            }
            finally
            {
                _isLoadingAccounts = false;
            }
        }

        private async Task LoadFoldersAsync(bool refresh = false)
        {
            if (_isLoadingFolders && !refresh) return;

            _isLoadingFolders = true;
            try
            {
                if (refresh)
                {
                    _currentFolderPage = 0;
                    _hasMoreFolders = true;

                    await MainThread.InvokeOnMainThreadAsync(() => Folders.Clear());
                }

                if (!_hasMoreFolders) return;

                var folders = await _cacheService.GetOrSetAsync(
                    $"folders_page_{_currentFolderPage}",
                    () => _folderDatabase.GetFoldersPagedAsync(_currentFolderPage * PageSize, PageSize),
                    TimeSpan.FromMinutes(5));

                if (folders.Count < PageSize)
                    _hasMoreFolders = false;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var folder in folders)
                    {
                        Folders.Add(folder);
                    }
                });

                _currentFolderPage++;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar pastas: {ex.Message}", "OK");
            }
            finally
            {
                _isLoadingFolders = false;
            }
        }

        private async Task LoadMoreAccountsAsync()
        {
            if (SelectedTab != "Itens") return;
            await LoadAccountsAsync();
        }

        private async Task LoadMoreFoldersAsync()
        {
            if (SelectedTab != "Pastas") return;
            await LoadFoldersAsync();
        }

        // Message handlers
        public void Receive(AccountSavedMessage message)
        {
            if (message.Value)
            {
                _cacheService.ClearAccountsCache();
                _ = Task.Run(() => LoadAccountsAsync(refresh: true));
            }
        }

        public void Receive(FolderSavedMessage message)
        {
            if (message.Value)
            {
                _cacheService.ClearFoldersCache();
                _ = Task.Run(() => LoadFoldersAsync(refresh: true));
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
                Folders?.Clear();

                _disposed = true;
            }
        }

        ~MainPageViewModel()
        {
            Dispose(false);
        }
    }
}