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

            // Adicionar handler para quando a página for exibida novamente
            window.Resumed += OnWindowResumed;

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

        private void OnWindowResumed(object sender, EventArgs e)
        {
            // Quando a janela for resumida, forçar uma atualização se necessário
            // Isso ajuda com problemas de navegação
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Se estiver na MainPage, forçar refresh
                    if (Shell.Current?.CurrentPage?.GetType().Name == "MainPage")
                    {
                        var currentPage = Shell.Current.CurrentPage;
                        if (currentPage?.BindingContext is ViewModels.MainPageViewModel mainViewModel)
                        {
                            // Pequeno delay para garantir que a navegação terminou
                            await Task.Delay(100);
                            await mainViewModel.RefreshCommand?.ExecuteAsync(null);
                        }
                    }
                }
                catch
                {
                    // Ignorar erros de navegação
                }
            });
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