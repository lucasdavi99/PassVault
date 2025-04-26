using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Views;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific;

namespace PassVault.ViewModels
{
    public partial class MainPageViewModel : ObservableObject, IRecipient<AccountSavedMessage>, IRecipient<FolderSavedMessage>
    {
        private readonly AccountDatabase _database;
        private readonly FolderDatabase _folderDatabase;

        [ObservableProperty]
        private string _selectedTab;

        [ObservableProperty]
        private int _tabPosition;

        [ObservableProperty]
        private string _selectedAction;

        [ObservableProperty]
        private ObservableCollection<Account> _accounts;

        [ObservableProperty]
        private ObservableCollection<Folder> _folders;

        public IRelayCommand SelectTabCommand { get; }

        public MainPageViewModel(AccountDatabase database, FolderDatabase folderDatabase)
        {
            _database = database;
            _folderDatabase = folderDatabase;

            SelectTabCommand = new AsyncRelayCommand<string>(OnTabSelected);
            SelectedTab = "Itens";
            TabPosition = 0;

            WeakReferenceMessenger.Default.Register<AccountSavedMessage>(this);
            WeakReferenceMessenger.Default.Register<FolderSavedMessage>(this);

            _ = LoadAccounts();
            _ = LoadFolders();
        }

        partial void OnTabPositionChanged(int value)
        {
            SelectedTab = value == 0 ? "Itens" : "Pastas";
            _ = RefreshCurrentTab();
        }

        private async Task RefreshCurrentTab()
        {
            if (SelectedTab == "Itens")
                await LoadAccounts();
            else if (SelectedTab == "Pastas")
                await LoadFolders();
        }

        private async Task OnTabSelected(string tab)
        {
            await Task.Delay(100);
            SelectedTab = tab;
            TabPosition = tab == "Itens" ? 0 : 1;

            await RefreshCurrentTab();
        }

        [RelayCommand]
        private async Task SelectAction(string action)
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

        [RelayCommand]
        private async Task EditAccount(Account account)
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
        private async Task DeleteAccount(Account account)
        {
            if (account != null)
            {
                bool confirm = await Shell.Current.DisplayAlert("Confirmação", "Deseja realmente excluir este item?", "Sim", "Não");

                if (confirm)
                {
                    await _database.DeleteAccountAsync(account);
                    await LoadAccounts();
                    await Shell.Current.DisplayAlert("Sucesso", "Conta excluída com sucesso.", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task DeleteFolder(Folder folder)
        {
            if (folder != null)
            {
                bool confirm = await Shell.Current.DisplayAlert("Confirmação", "Deseja realmente excluir essa pasta? Todos os itens dentro da pasta serão excluidos", "Sim", "Não");

                if (confirm)
                {
                    await _folderDatabase.DeleteFolderAsync(folder);
                    await LoadFolders();
                    await Shell.Current.DisplayAlert("Sucesso", "Pasta excluída com sucesso.", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task OpenFolderAsync(Folder folder)
        {
            await Shell.Current.GoToAsync($"{nameof(FolderPage)}?folderId={folder.Id}");
        }

        [RelayCommand]
        private async Task Help() => await Shell.Current.DisplayAlert("Ajuda", "Para deletar uma conta ou pasta, arraste para o lado esquerdo.", "OK");

        private static async Task SimulateAsyncWork(string message)
        {
            await Task.Delay(500);
            Console.WriteLine(message);
        }

        public void Receive(AccountSavedMessage message)
        {
            if (message.Value)
            {
                _ = LoadAccounts();
            }
        }

        public void Receive(FolderSavedMessage message)
        {
            if (message.Value)
            {
                _ = LoadFolders();
            }
        }

        private async Task LoadAccounts()
        {
            var accounts = await _database.GetAccountsAsync();
            var filteredAccounts = accounts.Where(account => account.FolderId == null).ToList();
            Accounts = new ObservableCollection<Account>(filteredAccounts);
        }

        private async Task LoadFolders()
        {
            var folders = await _folderDatabase.GetFoldersAsync();
            Folders = new ObservableCollection<Folder>(folders);
        }
    }
}