using FarEdit.Core.ViewModels.MainWindowViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace FarEdit.DependencyInjection;

public static class ViewModelBootstrapper
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<MainWindowViewModel>();
    }
}