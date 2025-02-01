using CommunityToolkit.Mvvm.Input;
using PassVault.Views;
using System.Windows.Input;

namespace PassVault.ViewModels
{
    internal class TutorialPage1ViewModel
    {
        public string Title => "Bem Vindo\r\n ao \r\nPassVault";
        public string Description => "Aqui você pode criar e registrar suas senhas de forma segura.";
        public ICommand NextPageCommand { get; }

        public TutorialPage1ViewModel()
        {
            NextPageCommand = new RelayCommand(OnNextPageClicked);
        }

        private async void OnNextPageClicked()
        {
            // Lógica para navegação (simulando com uma simples exibição de mensagem)
            await Shell.Current.GoToAsync(nameof(TutorialPage2));
        }
    }
}
