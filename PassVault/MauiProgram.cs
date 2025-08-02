using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using PassVault.Data;
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
                    fonts.AddFont("Roboto.ttf", "Roboto");
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

            // Database services - mantém como Singleton
            builder.Services.AddSingleton<AccountDatabase>();
            builder.Services.AddSingleton<FolderDatabase>();

            // Import/Export services como Singleton (são leves)
            builder.Services.AddSingleton<ExportService>();
            builder.Services.AddSingleton<ImportService>();

            // ViewModels como Transient para melhor gestão de memória
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

            // Pages com ViewModels
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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Build da aplicação
            var app = builder.Build();

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