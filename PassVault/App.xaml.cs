using PassVault.Services;

namespace PassVault
{
    public partial class App : Application
    {
        private readonly InactivityTimeoutService _inactivityService;

        public App()
        {
            InitializeComponent();

            _inactivityService = new InactivityTimeoutService();
            _inactivityService.TimeoutElapsed += OnInactivityTimeout;
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = new Window(new AppShell());
            return window;
        }

        protected override async void OnStart()
        {
            base.OnStart();

            bool isNewUser = Preferences.Get("IsNewUser", true);

            if (isNewUser)
                await Shell.Current.GoToAsync("//TutorialPage1");
            else
                await Shell.Current.GoToAsync("//LockScreen");
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            _inactivityService.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _inactivityService.Stop();
        }

        private async void OnInactivityTimeout()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.GoToAsync("//LockScreen");
            });
        }
    }
}
