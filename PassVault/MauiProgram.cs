using Microsoft.Extensions.Logging;
using Plugin.Fingerprint.Abstractions;
using Plugin.Fingerprint;
using PassVault.Data;
using PassVault.ViewModels;
using PassVault.Views;

namespace PassVault
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Roboto.ttf", "Roboto");  
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");  
                });

#if DEBUG
    		builder.Logging.AddDebug();
            builder.Services.AddSingleton<AccountDatabase>();
            builder.Services.AddTransient<NewAccountPageViewModel>();
            builder.Services.AddTransient<NewAccountPage>();
            builder.Services.AddTransient<PasswordGeneratorViewModel>();
            builder.Services.AddTransient<PasswordGenerator>();

#endif

            return builder.Build();
        }
    }
}
