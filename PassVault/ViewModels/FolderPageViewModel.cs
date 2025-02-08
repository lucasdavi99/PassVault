using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Models;
using PassVault.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.ViewModels
{
    public partial class FolderPageViewModel : ObservableObject
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
        }        

        [RelayCommand]
        public async Task AddNewItemAsync()
        {
            await Shell.Current.GoToAsync($"{nameof(NewAccountPage)}?folderId={FolderId}", true);
        }

        [RelayCommand]
        public async Task GoToHome()
        {
            await Shell.Current.GoToAsync($"{nameof(MainPage)}");
        }

        public async Task LoadDataAsync()
        {
            Folder = await _folderDatabase.GetFolderAsync(FolderId);
            var items = await _accountDatabase.GetAccountsByFolderIdAsync(FolderId);
            Accounts = new ObservableCollection<Account>(items);
        }
    }
}