using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;
using Microsoft.Extensions.DependencyInjection;
namespace FarEdit.Core.ViewModels.MainWindowViewModel;

public static class MainWindowViewModelRegistrations
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<GetFarFiles.Handler>();
    }
}