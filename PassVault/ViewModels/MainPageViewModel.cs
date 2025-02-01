using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassVault.Data;
using PassVault.Messages;
using PassVault.Models;
using PassVault.Views;
using System.Collections.ObjectModel;

namespace PassVault.ViewModels
{
    public partial class MainPageViewModel : ObservableObject, IRecipient<AccountSavedMessage>
    {
        private readonly AccountDatabase _database;

        [ObservableProperty]
        private string _selectedTab;
        [ObservableProperty]
        private string _selectedAction;
        [ObservableProperty]
        private ObservableCollection<Account> _accounts;


        public IRelayCommand SelectTabCommand { get; }
        public IRelayCommand<Account> EditAccountCommand { get; }

        public MainPageViewModel(AccountDatabase database)
        {
            _database = database;
            SelectTabCommand = new AsyncRelayCommand<string>(OnTabSelected);
            SelectedTab = "Itens";

            WeakReferenceMessenger.Default.Register(this);

            LoadAccounts();
            EditAccountCommand = new RelayCommand<Account>(OnAccountSelected);
        }

        private async Task OnTabSelected(string tab)
        {
            await Task.Delay(100);
            SelectedTab = tab;

            if(tab == "Itens") await LoadAccounts();
        }
        
        [RelayCommand]
        private async Task SelectAction(string action)
        {
            switch (action)
            {
                case "Home":
                    // Lógica para o botão Home
                    await SimulateAsyncWork("Home selecionado");
                    break;

                case "Add":
                    // Lógica para o botão Adicionar
                    await Shell.Current.GoToAsync(nameof(NewAccountPage));
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

        private async Task LoadAccounts()
        {
            var accounts = await _database.GetAccountsAsync();
            Accounts = new ObservableCollection<Account>(accounts);
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
                LoadAccounts();
            }
        }
    }
}