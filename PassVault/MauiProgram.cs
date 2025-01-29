using Microsoft.Extensions.Logging;
using Plugin.Fingerprint.Abstractions;
using Plugin.Fingerprint;

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
            //builder.Services.AddSingleton<>
#endif

            return builder.Build();
        }
    }
}
