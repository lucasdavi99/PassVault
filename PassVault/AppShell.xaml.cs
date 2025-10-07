using PassVault.Views;

namespace PassVault
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();

            // Adicionar handler para navegação
            Navigated += OnShellNavigated;
        }

        private void RegisterRoutes()
        {
            // Registrar todas as rotas
            Routing.RegisterRoute(nameof(NewAccountPage), typeof(NewAccountPage));
            Routing.RegisterRoute(nameof(NewFolderPage), typeof(NewFolderPage));
            Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
            Routing.RegisterRoute(nameof(FolderPage), typeof(FolderPage));
            Routing.RegisterRoute(nameof(PasswordGenerator), typeof(PasswordGenerator));
            Routing.RegisterRoute(nameof(EditAccountPage), typeof(EditAccountPage));
            Routing.RegisterRoute(nameof(EditFolderPage), typeof(EditFolderPage));
            Routing.RegisterRoute(nameof(BackupPage), typeof(BackupPage));
            Routing.RegisterRoute(nameof(FieldsSelection), typeof(FieldsSelection));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(UpgradePage), typeof(UpgradePage));

            // Páginas do tutorial
            Routing.RegisterRoute(nameof(TutorialPage2), typeof(TutorialPage2));
            Routing.RegisterRoute(nameof(TutorialPage3), typeof(TutorialPage3));
            Routing.RegisterRoute(nameof(TutorialPage4), typeof(TutorialPage4));
            Routing.RegisterRoute(nameof(TutorialPage5), typeof(TutorialPage5));
        }

        private async void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            // Quando navegar de volta para MainPage, forçar refresh
            if (e.Current?.Location?.ToString().Contains("MainPage") == true)
            {
                await Task.Delay(200);  
            }

            // Quando navegar de volta para FolderPage, forçar refresh
            if (e.Current?.Location?.ToString().Contains("FolderPage") == true)
            {
                await Task.Delay(200);

                var currentPage = CurrentPage;
                if (currentPage?.BindingContext is ViewModels.FolderPageViewModel folderViewModel)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            await folderViewModel.LoadDataAsync();
                        }
                        catch
                        {
                            // Ignorar erros
                        }
                    });
                }
            }
        }
    }
}