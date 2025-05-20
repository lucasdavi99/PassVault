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
    }
}