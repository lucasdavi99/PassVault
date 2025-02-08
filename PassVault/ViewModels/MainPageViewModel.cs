﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Views;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace PassVault.ViewModels
{
    public partial class MainPageViewModel : ObservableObject, IRecipient<AccountSavedMessage>, IRecipient<FolderSavedMessage>
    {
        private readonly AccountDatabase _database;
        private readonly FolderDatabase _folderDatabase;

        [ObservableProperty]
        private string _selectedTab;
        [ObservableProperty]
        private string _selectedAction;
        [ObservableProperty]
        private ObservableCollection<Account> _accounts;
        [ObservableProperty]
        private ObservableCollection<Folder> _folders;


        public IRelayCommand SelectTabCommand { get; }
        public IRelayCommand<Account> EditAccountCommand { get; }

        public MainPageViewModel(AccountDatabase database, FolderDatabase folderDatabase)
        {
            _database = database;
            _folderDatabase = folderDatabase;

            SelectTabCommand = new AsyncRelayCommand<string>(OnTabSelected);
            EditAccountCommand = new RelayCommand<Account>(OnAccountSelected);
            SelectedTab = "Itens";

            WeakReferenceMessenger.Default.Register<AccountSavedMessage>(this);
            WeakReferenceMessenger.Default.Register<FolderSavedMessage>(this);

            _ = LoadAccounts();
            _ = LoadFolders();
        }

        private async Task OnTabSelected(string tab)
        {
            await Task.Delay(100);
            SelectedTab = tab;

            if(tab == "Itens") await LoadAccounts();
            if(tab == "Pastas") await LoadFolders();
        }
        
        [RelayCommand]
        private async Task SelectAction(string action)
        {
            switch (action)
            {
                case "Export/Import":
                    // Lógica para o botão Home
                    await Shell.Current.DisplayActionSheet("Deseja exportar ou importar seu backup?", "Cancelar", null, "Exportar", "Importar");
                    break;

                case "Add":
                    if (SelectedTab == "Itens")
                    {
                        await Shell.Current.GoToAsync(nameof(NewAccountPage));
                    }
                    else if (SelectedTab == "Pastas")
                    {
                        await Shell.Current.GoToAsync(nameof(NewFolderPage));
                    }                    
                    break;

                case "Search":
                    // Lógica para o botão Procurar
                    await SimulateAsyncWork("Procurar selecionado");
                    break;
            }
        }

        [RelayCommand]
        private async void OnAccountSelected(Account account)
        {
            await Shell.Current.GoToAsync($"{nameof(EditAccountPage)}?accountId={account.Id}");
        }       

        [RelayCommand]
        private async Task DeleteAccount(Account account)
        {
            if(account != null)
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
            Console.WriteLine($"Número de contas: {Accounts.Count}");
        }

        private async Task LoadFolders()
        {
            var folders = await _folderDatabase.GetFoldersAsync();
            Folders = new ObservableCollection<Folder>(folders);
            Console.WriteLine($"Número de contas: {Folders.Count}");
        }
    }
}