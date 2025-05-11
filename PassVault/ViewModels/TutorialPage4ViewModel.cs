using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PassVault.Views;

namespace PassVault.ViewModels
{
    class TutorialPage4ViewModel
    {
        public string Title => "Informação!";
        public string Description => "Este aplicativo foi desenvolvido para proporcionar segurança e praticidade no armazenamento de suas senhas.\r\n\r\nTodas as informações são armazenadas exclusivamente no seu dispositivo e nunca são compartilhadas.";
        public ICommand NextPageCommand { get; }

        public TutorialPage4ViewModel()
        {
            NextPageCommand = new RelayCommand(OnNextPageClicked);
        }

        private async void OnNextPageClicked()
        {
            await Shell.Current.GoToAsync(nameof(TutorialPage5));
        }
    }
}
