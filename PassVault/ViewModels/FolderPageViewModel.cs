using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OrionVault.Data;
using OrionVault.Interfaces;
using OrionVault.Messages;
using OrionVault.Models;
using OrionVault.Services;
using OrionVault.Views;

namespace OrionVault.ViewModels
{
    public partial class FolderPageViewModel : ObservableObject, IQueryAttributable, IRecipient<AccountSavedMessage>, IRecipient<FolderSavedMessage>, IDisposable
    {
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;
        private readonly ILocalizationService _localizationService;

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

        // Localized Properties
        [ObservableProperty] private string pageSubtitle;
        [ObservableProperty] private string accountsTabText;
        [ObservableProperty] private string foldersTabText;
        [ObservableProperty] private string emptyTitle;
        [ObservableProperty] private string emptyMessage;
        [ObservableProperty] private string createdAtFormat;
        [ObservableProperty] private string subfolderItemSubtitle;
        [ObservableProperty] private string deleteText;

        public FolderPageViewModel(AccountDatabase accountDatabase, FolderDatabase folderDatabase, ILocalizationService localizationService)
        {
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
            _localizationService = localizationService;
            Accounts = new ObservableCollection<Account>();
            SubFolders = new ObservableCollection<Folder>();

            // Registrar para receber mensagens
            WeakReferenceMessenger.Default.Register<AccountSavedMessage>(this);
            WeakReferenceMessenger.Default.Register<FolderSavedMessage>(this);
            _localizationService.LanguageChanged += OnLanguageChanged;

            UpdateLocalizedTexts();
            UpdateVisibilityProperties();
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            UpdateLocalizedTexts();
            FormatAccountDates(Accounts);
        }

        private void UpdateLocalizedTexts()
        {
            PageSubtitle = L.Text("folder_page.page_subtitle");
            AccountsTabText = L.Text("folder_page.accounts_tab");
            FoldersTabText = L.Text("folder_page.folders_tab");
            EmptyTitle = L.Text("folder_page.empty_folder_title");
            EmptyMessage = L.Text("folder_page.empty_folder_message");
            CreatedAtFormat = L.Text("main.created_at_format");
            SubfolderItemSubtitle = L.Text("folder_page.subfolder_item_subtitle");
            DeleteText = L.Text("common.delete");
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
                await Shell.Current.DisplayAlert(L.Text("common.error"), string.Format(L.Text("folder_page.error_opening_folder"), ex.Message), L.Text("common.ok"));
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
                bool confirm = await Shell.Current.DisplayAlert(
                    L.Text("folder_page.delete_account_confirm_title"),
                    L.Text("folder_page.delete_account_confirm_message"),
                    L.Text("common.yes"),
                    L.Text("common.no"));

                if (confirm)
                {
                    await _accountDatabase.DeleteAccountAsync(account);
                    Accounts.Remove(account);
                    await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("folder_page.delete_account_success"), L.Text("common.ok"));
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
                bool confirm = await Shell.Current.DisplayAlert(
                    L.Text("folder_page.delete_account_confirm_title"),
                    L.Text("folder_page.delete_folder_confirm_message"),
                    L.Text("common.yes"),
                    L.Text("common.no"));

                if (confirm)
                {
                    await _folderDatabase.DeleteFolderAsync(subFolder);
                    SubFolders.Remove(subFolder);
                    await Shell.Current.DisplayAlert(L.Text("common.success"), L.Text("folder_page.delete_folder_success"), L.Text("common.ok"));
                    UpdateVisibilityProperties();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(L.Text("common.error"), string.Format(L.Text("folder_page.error_deleting_folder"), ex.Message), L.Text("common.ok"));
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
                FormatAccountDates(Accounts);

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
                await Shell.Current.DisplayAlert(L.Text("common.error"), string.Format(L.Text("folder_page.error_loading_data"), ex.Message), L.Text("common.ok"));
            }
        }

        private void FormatAccountDates(IEnumerable<Account> accounts)
        {
            if (accounts == null) return;
            foreach (var account in accounts)
            {
                account.FormattedCreatedDate = string.Format(CreatedAtFormat, account.Created);
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
                _localizationService.LanguageChanged -= OnLanguageChanged;
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