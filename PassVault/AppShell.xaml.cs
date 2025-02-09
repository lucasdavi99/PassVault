using Microsoft.Maui.Controls;
using PassVault.Views;

namespace PassVault
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(NewAccountPage), typeof(NewAccountPage));
            Routing.RegisterRoute(nameof(NewFolderPage), typeof(NewFolderPage));
            Routing.RegisterRoute(nameof(FolderPage), typeof(FolderPage));
            Routing.RegisterRoute(nameof(PasswordGenerator), typeof(PasswordGenerator));
            Routing.RegisterRoute(nameof(EditAccountPage), typeof(EditAccountPage));
            Routing.RegisterRoute(nameof(EditFolderPage), typeof(EditFolderPage));


            //Rotas das paginas de Tutorial
            Routing.RegisterRoute(nameof(TutorialPage1), typeof(TutorialPage1));
            Routing.RegisterRoute(nameof(TutorialPage2), typeof(TutorialPage2));
            Routing.RegisterRoute(nameof(TutorialPage3), typeof(TutorialPage3));
            Routing.RegisterRoute(nameof(TutorialPage4), typeof(TutorialPage4));
            Routing.RegisterRoute(nameof(TutorialPage5), typeof(TutorialPage5));
        }
    }
}
