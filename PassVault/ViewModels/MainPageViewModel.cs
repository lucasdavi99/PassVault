using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Services;
using PassVault.Views;
using System.Collections.ObjectModel;

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

        [ObservableProperty]
        private bool isEmpty;

        private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

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
        }

        private async Task OnTabSelected(string tab)
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
        private async Task AddNewItemAsync()
        {
            await Shell.Current.GoToAsync(nameof(FieldsSelection), true);
        }

        [RelayCommand]
        private async Task AddNewFolderAsync()
        {
            await Shell.Current.GoToAsync(nameof(NewFolderPage), true);
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

                    // Remover da coleção local imediatamente
                    await MainThread.InvokeOnMainThreadAsync(() => Accounts.Remove(account));

                    // Limpar cache
                    _cacheService.ClearAccountsCache();

                    await Shell.Current.DisplayAlert("Sucesso", "Conta excluída com sucesso.", "OK");
                    UpdateEmptyState();
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
                bool confirm = await Shell.Current.DisplayAlert("Confirmação", "Deseja realmente excluir essa pasta? Todos os itens e subpastas dentro da pasta serão excluidos", "Sim", "Não");

                if (confirm)
                {
                    await _folderDatabase.DeleteFolderAsync(folder);

                    // Remover da coleção local imediatamente
                    await MainThread.InvokeOnMainThreadAsync(() => Folders.Remove(folder));

                    // Limpar cache
                    _cacheService.ClearFoldersCache();
                    _cacheService.ClearAccountsCache(); // Contas também podem ter sido afetadas

                    await Shell.Current.DisplayAlert("Sucesso", "Pasta excluída com sucesso.", "OK");
                    UpdateEmptyState();
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
        private async Task Help() => await Shell.Current.DisplayAlert("Ajuda", "Para deletar uma conta ou pasta, arraste para o lado esquerdo.", "OK");

        [RelayCommand]
        private async Task GoToSettings()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(SettingsPage));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar para configurações: {ex.Message}", "OK");
            }
        }

        // Métodos de carregamento com paginação
        private async Task LoadAccountsAsync(bool refresh = false)
        {
            if (_isLoadingAccounts || (!refresh && !_hasMoreAccounts))
                return;

            _isLoadingAccounts = true;

            try
            {
                if (refresh)
                {
                    _currentAccountPage = 0;
                    _hasMoreAccounts = true;

                    await MainThread.InvokeOnMainThreadAsync(() => Accounts.Clear());
                }

                // Carregar apenas contas sem pasta (da MainPage)
                var newAccounts = await _database.GetAccountsWithoutFolderAsync(_currentAccountPage * PageSize, PageSize);

                _hasMoreAccounts = newAccounts.Count == PageSize;
                _currentAccountPage++;

                var sortedAccounts = newAccounts
                    .OrderBy(account => account.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var account in sortedAccounts)
                    {
                        Accounts.Add(account);
                    }
                    UpdateEmptyState();
                });
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
            if (_isLoadingFolders || (!refresh && !_hasMoreFolders))
                return;

            _isLoadingFolders = true;

            try
            {
                if (refresh)
                {
                    _currentFolderPage = 0;
                    _hasMoreFolders = true;

                    await MainThread.InvokeOnMainThreadAsync(() => Folders.Clear());
                }

                // Carregar apenas pastas raiz (sem pai)
                var newFolders = await _folderDatabase.GetFoldersPagedAsync(_currentFolderPage * PageSize, PageSize);

                _hasMoreFolders = newFolders.Count == PageSize;
                _currentFolderPage++;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var folder in newFolders)
                    {
                        Folders.Add(folder);
                    }
                    UpdateEmptyState();
                });
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
            if (SelectedTab == "Itens")
            {
                await LoadAccountsAsync();
            }
        }

        private async Task LoadMoreFoldersAsync()
        {
            if (SelectedTab == "Pastas")
            {
                await LoadFoldersAsync();
            }
        }

        private async Task RefreshCurrentTabAsync()
        {
            if (_refreshSemaphore.CurrentCount == 0)
                return;

            await _refreshSemaphore.WaitAsync();

            try
            {
                IsRefreshing = true;

                if (SelectedTab == "Itens")
                {
                    await LoadAccountsAsync(refresh: true);
                }
                else if (SelectedTab == "Pastas")
                {
                    await LoadFoldersAsync(refresh: true);
                }
            }
            finally
            {
                IsRefreshing = false;
                _refreshSemaphore.Release();
            }
        }

        private void UpdateEmptyState()
        {
            IsEmpty = (Accounts?.Count ?? 0) == 0 && (Folders?.Count ?? 0) == 0;
        }

        public async void Receive(AccountSavedMessage message)
        {
            if (message.Value)
            {
                await RefreshCurrentTabAsync();
            }
        }

        public async void Receive(FolderSavedMessage message)
        {
            if (message.Value)
            {
                await RefreshCurrentTabAsync();
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
                _refreshSemaphore?.Dispose();
                _disposed = true;
            }
        }

        ~MainPageViewModel()
        {
            Dispose(false);
        }
    }
}