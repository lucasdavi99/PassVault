using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassVault.Data;
using PassVault.Models;
using System.Collections.ObjectModel;

namespace PassVault.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly AccountDatabase _database;

        [ObservableProperty]
        private string _selectedTab;
        [ObservableProperty]
        private string _selectedAction;
        [ObservableProperty]
        private ObservableCollection<Account> _accounts;


        public IRelayCommand SelectTabCommand { get; }

        public MainPageViewModel(AccountDatabase database)
        {
            _database = database;
            SelectTabCommand = new AsyncRelayCommand<string>(OnTabSelected);
            SelectedTab = "Itens";

            LoadAccounts();
        }

        private async Task OnTabSelected(string tab)
        {
            await Task.Delay(100);
            SelectedTab = tab;

            if(tab == "Itens") LoadAccounts();
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
                    await Shell.Current.GoToAsync("///NewAccountPage");
                    break;

                case "Search":
                    // Lógica para o botão Procurar
                    await SimulateAsyncWork("Procurar selecionado");
                    break;
            }
        }

        private async void LoadAccounts()
        {
            var accounts = await _database.GetAccountsAsync();
            Accounts = new ObservableCollection<Account>(accounts);
        }

        private async Task SimulateAsyncWork(string message)
        {
            await Task.Delay(500); // Simulação de uma operação demorada
            Console.WriteLine(message);
        }
    }
}