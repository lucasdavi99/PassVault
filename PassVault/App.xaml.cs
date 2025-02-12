using PassVault.Views;

namespace PassVault
{
    public partial class App : Application
    {
        public App()
        {
            Preferences.Clear();

            bool isNewUser = Preferences.Get("IsNewUser", true);

            if (isNewUser)
            {
                MainPage = new AppShell();
                Shell.Current.GoToAsync(nameof(TutorialPage1));
            }
            else
            {
                MainPage = new AppShell();
                Shell.Current.GoToAsync(nameof(MainPage));
            }
        }
    }
}
