using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PassVault.ViewModels
{
    class TutorialPage4ViewModel
    {
        public string Title => "Informação!";
        public string Description => "Este aplicativo foi desenvolvido para proporcionar segurança e praticidade no armazenamento de suas senhas.\r\n\r\nTodas as informações são armazenadas exclusivamente no seu dispositivo e nunca são compartilhadas.\r\n\r\nPara garantir máxima proteção, utilizamos a senha padrão do seu smartphone, eliminando a necessidade de armazená-la na nuvem para acessar suas informações.";
        public ICommand NextPageCommand { get; }

        public TutorialPage4ViewModel()
        {
            NextPageCommand = new RelayCommand(OnNextPageClicked);
        }

        private async void OnNextPageClicked()
        {
            await Shell.Current.GoToAsync("///TutorialPage5", true);
        }
    }
}
