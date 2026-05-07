using Kosmozeki.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kosmozeki
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
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "kosmozeki.db");

            builder.Services.AddDbContext<Kosmozeki.Core.Data.AppDbContext>(options =>
                options.UseSqlite($"Filename={dbPath}")
            );

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<CombatEngineService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
