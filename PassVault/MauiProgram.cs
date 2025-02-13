using Microsoft.Extensions.Logging;
using Plugin.Fingerprint.Abstractions;
using Plugin.Fingerprint;
using PassVault.Data;
using PassVault.ViewModels;
using PassVault.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui;
using PassVault.Services;

namespace PassVault
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Roboto.ttf", "Roboto");  
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");  
                });

            builder.Services.AddSingleton<AccountDatabase>();
            builder.Services.AddSingleton<FolderDatabase>();
            builder.Services.AddSingleton<ExportService>();
            builder.Services.AddSingleton<ImportService>();            

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<NewAccountPage>();
            builder.Services.AddTransient<NewAccountPageViewModel>();
            builder.Services.AddTransient<PasswordGenerator>();
            builder.Services.AddTransient<PasswordGeneratorViewModel>();
            builder.Services.AddTransient<EditAccountPage>();
            builder.Services.AddTransient<EditAccountPageViewModel>();
            builder.Services.AddTransient<NewFolderPage>();
            builder.Services.AddTransient<NewFolderPageViewModel>();
            builder.Services.AddTransient<FolderPage>();
            builder.Services.AddTransient<FolderPageViewModel>();
            builder.Services.AddTransient<EditFolderPage>();
            builder.Services.AddTransient<EditFolderPageViewModel>();
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<SearchPageViewModel>();
            builder.Services.AddTransient<BackupPage>();
            builder.Services.AddTransient<BackupViewModel>();          

#if DEBUG
            builder.Logging.AddDebug();            

#if DEBUG
    		builder.Logging.AddDebug();

#endif

            return builder.Build();
        }
    }
}
