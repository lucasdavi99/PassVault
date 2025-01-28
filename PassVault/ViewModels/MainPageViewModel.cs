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
            SelectTabCommand = new AsyncRelayCommand<string>(OnTabSelected);
            SelectedTab = "Itens";
        }

        private async Task OnTabSelected(string tab)
        {
            await Task.Delay(100);
            SelectedTab = tab;
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
                    await SimulateAsyncWork("Adicionar selecionado");
                    break;

                case "Search":
                    // Lógica para o botão Procurar
                    await SimulateAsyncWork("Procurar selecionado");
                    break;
            }
        }

        private async Task SimulateAsyncWork(string message)
        {
            await Task.Delay(500); // Simulação de uma operação demorada
            Console.WriteLine(message);
        }
    }
}