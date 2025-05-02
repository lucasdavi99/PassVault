using PassVault.Views;

namespace PassVault
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Atraso para garantir que o Shell está completamente inicializado
            await Task.Delay(100);

            bool isNewUser = Preferences.Get("IsNewUser", true);

            if (isNewUser)
                await Shell.Current.GoToAsync("//TutorialPage1");
            else
                await Shell.Current.GoToAsync("//LockScreen");
        }
    }
}