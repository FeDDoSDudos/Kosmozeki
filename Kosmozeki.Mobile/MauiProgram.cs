using Kosmozeki.Application.DependencyInjection;
using Kosmozeki.Core.Services;
using Kosmozeki.Infrastructure.DependencyInjection;
using Kosmozeki.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace Kosmozeki.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "kosmozeki.db");

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();
        builder.Services.AddCache();
        builder.Services.AddKosmozekiMauiInfrastructure(dbPath);

        builder.Services.AddScoped<CombatFacade>();
        builder.Services.AddScoped<NotesFacade>();

        builder.Services.AddSingleton<CombatEngineService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}