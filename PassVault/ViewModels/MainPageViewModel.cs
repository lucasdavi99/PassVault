using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PassVault.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _selectedTab;

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
    }
}