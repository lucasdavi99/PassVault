using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace PassVault.ViewModels
{
    class TutorialPage5ViewModel
    {
        public string Title => "Pronto!\r\nAgora é só aproveitar o app.";
        public ICommand NextPageCommand { get; }

        public TutorialPage5ViewModel()
        {
            NextPageCommand = new RelayCommand(OnNextPageClicked);
        }

        private async void OnNextPageClicked()
        {
            Preferences.Set("IsNewUser", false);
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
