using Microsoft.Maui.Controls;
using PassVault.Views;

namespace PassVault
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            RegisterRoutes();
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

            // Páginas do tutorial
            Routing.RegisterRoute(nameof(TutorialPage2), typeof(TutorialPage2));
            Routing.RegisterRoute(nameof(TutorialPage3), typeof(TutorialPage3));
            Routing.RegisterRoute(nameof(TutorialPage4), typeof(TutorialPage4));
            Routing.RegisterRoute(nameof(TutorialPage5), typeof(TutorialPage5));
        }
    }
}