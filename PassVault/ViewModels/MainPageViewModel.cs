using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PassVault.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _selectedTab;
        [ObservableProperty]
        private string _selectedAction;


        public IRelayCommand SelectTabCommand { get; }

        public MainPageViewModel()
        {
            SelectTabCommand = new RelayCommand<string>(OnTabSelected);
            SelectedTab = "Itens";
        }

        private void OnTabSelected(string tab)
        {
            SelectedTab = tab;
        }

        [RelayCommand]
        private void SelectAction(string action)
        {
            SelectedAction = action;

            // Executa a ação correspondente
            if (action == "Item")
            {
                // Lógica para criar novo item
                Console.WriteLine("Criando novo item...");
            }
            else if (action == "Pasta")
            {
                // Lógica para criar nova pasta
                Console.WriteLine("Criando nova pasta...");
            }
        }
    }
}