using CommunityToolkit.Maui;
using PkMechScheduler.Frontend.Services;

namespace PkMechScheduler.Frontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.Services.AddSingleton<SerializerService>();
        builder.Services.AddSingleton<ScheduleService>();

        return builder.Build();
    }
}