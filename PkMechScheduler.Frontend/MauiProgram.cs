﻿using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using PkMechScheduler.Database;
using PkMechScheduler.Frontend.Interfaces;
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
        builder.Services.AddDbContext<SchedulerContext>(x => x.UseSqlite("scheduler.db"));
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        return builder.Build();
    }
}