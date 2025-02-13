﻿using FarEdit.Core.ViewModels.MainWindowViewModel.Commands;
using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace FarEdit.Core.ViewModels.MainWindowViewModel;

public static class MainWindowViewModelRegistrations
{
    public static void Register(IServiceCollection services)
    {
        services
            .AddScoped<GetFarVm.Handler>()
            .AddScoped<SaveFar.Handler>()
            .AddScoped<ExportEntries.Handler>()
            .AddScoped<GetFarFilesFromPaths.Handler>();
    }
}
