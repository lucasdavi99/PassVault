using CommunityToolkit.Maui;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PassVault.Data;
using PassVault.Interfaces;
using PassVault.Services;
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
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Nunito-Medium.ttf", "Nunito-Medium");
                    fonts.AddFont("Nunito-Bold.ttf", "Nunito-Bold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // Configurações de performance
            builder.Services.Configure<MemoryCacheOptions>(options =>
            {
                options.SizeLimit = 100; // Limite de entradas no cache
                options.CompactionPercentage = 0.25; // Remove 25% quando atinge o limite
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
            });

            // Services como Singleton para melhor performance
            builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
            builder.Services.AddSingleton<CacheService>();

            // Serviço de Localização
            builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

            // Database services - mantém como Singleton
            builder.Services.AddSingleton<AccountDatabase>();
            builder.Services.AddSingleton<FolderDatabase>();

            // Import/Export services como Singleton (são leves)
            builder.Services.AddSingleton<ExportService>();
            builder.Services.AddSingleton<ImportService>();

            // ViewModels como Transient para melhor gestão de memória
            builder.Services.AddSingleton<TutorialPage1ViewModel>();
            builder.Services.AddSingleton<TutorialPage2ViewModel>();
            builder.Services.AddSingleton<TutorialPage3ViewModel>();
            builder.Services.AddSingleton<TutorialPage4ViewModel>();
            builder.Services.AddSingleton<TutorialPage5ViewModel>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<NewAccountPageViewModel>();
            builder.Services.AddTransient<EditAccountPageViewModel>();
            builder.Services.AddTransient<NewFolderPageViewModel>();
            builder.Services.AddTransient<FolderPageViewModel>();
            builder.Services.AddTransient<EditFolderPageViewModel>();
            builder.Services.AddTransient<PasswordGeneratorViewModel>();
            builder.Services.AddTransient<SearchPageViewModel>();
            builder.Services.AddTransient<BackupViewModel>();
            builder.Services.AddTransient<FieldsSelectionViewModel>();
            builder.Services.AddTransient<SettingsPageViewModel>();
            builder.Services.AddTransient<LockScreenViewModel>();

            // Pages com ViewModels
            builder.Services.AddTransient<TutorialPage1>();
            builder.Services.AddTransient<TutorialPage2>();
            builder.Services.AddTransient<TutorialPage3>();
            builder.Services.AddTransient<TutorialPage4>();
            builder.Services.AddTransient<TutorialPage5>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<NewAccountPage>();
            builder.Services.AddTransient<EditAccountPage>();
            builder.Services.AddTransient<FolderPage>();
            builder.Services.AddTransient<PasswordGenerator>();
            builder.Services.AddTransient<NewFolderPage>();
            builder.Services.AddTransient<EditFolderPage>();
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<BackupPage>();
            builder.Services.AddTransient<FieldsSelection>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<LockScreen>();

#if DEBUG
            builder.Logging.AddDebug();
            // Força o tutorial a ser exibido em modo de depuração para testes
            //Preferences.Set("IsNewUser", true);
#endif

            // Build da aplicação
            var app = builder.Build();

            // Inicializar o serviço de localização global
            var localizationService = app.Services.GetRequiredService<ILocalizationService>();
            L.Initialize(localizationService);

            // Inicialização em background dos services críticos
            Task.Run(async () =>
            {
                try
                {
                    using var scope = app.Services.CreateScope();
                    var cacheService = scope.ServiceProvider.GetRequiredService<CacheService>();

                    // Pre-warm cache com dados frequentemente acessados
                    var accountDb = scope.ServiceProvider.GetRequiredService<AccountDatabase>();
                    await cacheService.GetOrSetAsync("initial_accounts",
                        () => accountDb.GetAccountsWithoutFolderAsync(0, 10),
                        TimeSpan.FromMinutes(10));
                }
                catch (Exception ex)
                {
                    // Log error but don't crash app
                    System.Diagnostics.Debug.WriteLine($"Pre-warm cache failed: {ex.Message}");
                }
            });

            return app;
        }
    }
}