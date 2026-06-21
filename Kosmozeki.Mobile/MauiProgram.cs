using Kosmozeki.Application.DependencyInjection;
using Kosmozeki.Core.Realtime;
using Kosmozeki.Core.Realtime.Implementations;
using Kosmozeki.Core.Services;
using Kosmozeki.Domain.Shared;
using Kosmozeki.Infrastructure.DependencyInjection;
using Kosmozeki.Infrastructure.Messaging;
using Kosmozeki.Mobile.Options;
using Kosmozeki.Mobile.Services;
using Kosmozeki.Mobile.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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


        using var appSettingsStream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
        builder.Configuration.AddJsonStream(appSettingsStream);
        builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(ServerOptions.SectionName));

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "kosmozeki.db");

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();
        builder.Services.AddCache();
        builder.Services.AddKosmozekiMauiInfrastructure(dbPath);

        builder.Services.AddScoped<CombatFacade>();
        builder.Services.AddScoped<NotesFacade>();

        builder.Services.AddSingleton<CombatEngineService>();

        builder.Services.AddSingleton<IPlayerIdentity, PlayerIdentity>();

        builder.Services.AddSingleton<IRoomRealtimeService, RoomRealtimeService>();
        builder.Services.AddSingleton<IRoomContext, RoomContext>();
        builder.Services.AddSingleton<ISyncBackgroundService, SyncBackgroundService>();
        builder.Services.AddHttpClient<INotesSyncTransport, NotesSyncTransport>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ServerOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        builder.Services.AddSingleton<IInspirationState, InspirationState>();

        builder.Services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}