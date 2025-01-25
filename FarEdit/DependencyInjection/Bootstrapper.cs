using FarEdit.Core.ViewModels.MainWindowViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace FarEdit.DependencyInjection;

public static class Bootstrapper
{
    public static void Register(IServiceCollection services)
    {
        MainWindowViewModelRegistrations.Register(services);
        ViewModelBootstrapper.Register(services);
    }
}
