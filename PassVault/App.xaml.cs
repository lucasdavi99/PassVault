using PassVault.Views;

namespace PassVault
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var shell = new AppShell();

            shell.Dispatcher.Dispatch(async () =>
            {
                bool isNewUser = Preferences.Get("IsNewUser", true);

                if (isNewUser)
                    await shell.GoToAsync($"///{nameof(TutorialPage1)}");
                else
                    await shell.GoToAsync($"///{nameof(LockScreen)}");
            });

            return new Window(shell);
        }

    }
}
