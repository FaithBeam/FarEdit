using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FarEdit.Core.ViewModels;
using FarEdit.Core.ViewModels.MainWindowViewModel;
using FarEdit.DependencyInjection;
using FarEdit.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace FarEdit;

public partial class App : Application
{
    public IServiceProvider? Container { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Init();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Container?.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Init()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.UseMicrosoftDependencyResolver();

                var resolver = Locator.CurrentMutable;
                resolver.InitializeSplat();
                resolver.InitializeReactiveUI();

                Bootstrapper.Register(services);

                services.AddSingleton<
                    IActivationForViewFetcher,
                    AvaloniaActivationForViewFetcher
                >();
                services.AddSingleton<IPropertyBindingHook, AutoDataTemplateBindingHook>();
            })
            .Build();
        Container = host.Services;
        Container.UseMicrosoftDependencyResolver();

        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
    }
}
