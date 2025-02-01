using CommunityToolkit.Mvvm.Input;
using PassVault.Views;
using System.Windows.Input;

namespace PassVault.ViewModels
{
    class TutorialPage2ViewModel
    {
        public string Title => "Senhas mais seguras";
        public string Description => "Suas senhas serão armazenadas exclusivamente em seu dispositivo, protegidas por uma criptografia robusta.";
        public ICommand NextPageCommand { get; }

        public TutorialPage2ViewModel()
        {
            NextPageCommand = new RelayCommand(OnNextPageClicked);
        }

        private async void OnNextPageClicked()
        {
            await Shell.Current.GoToAsync(nameof(TutorialPage3));
        }
    }
}
