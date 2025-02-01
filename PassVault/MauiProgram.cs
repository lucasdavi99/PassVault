using Microsoft.Extensions.Logging;
using Plugin.Fingerprint.Abstractions;
using Plugin.Fingerprint;
using PassVault.Data;
using PassVault.ViewModels;
using PassVault.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;

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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Roboto.ttf", "Roboto");  
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");  
                });

#if DEBUG
    		builder.Logging.AddDebug();
            builder.Services.AddSingleton<AccountDatabase>();
            
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<NewAccountPageViewModel>();
            builder.Services.AddTransient<NewAccountPage>();
            builder.Services.AddTransient<PasswordGeneratorViewModel>();
            builder.Services.AddTransient<PasswordGenerator>();
            builder.Services.AddTransient<EditAccountPageViewModel>();
            builder.Services.AddTransient<EditAccountPage>();

#endif

            return builder.Build();
        }
    }
}
