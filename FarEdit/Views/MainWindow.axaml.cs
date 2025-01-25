using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using FarEdit.Core.ViewModels.MainWindowViewModel;
using ReactiveUI;

namespace FarEdit.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            if (ViewModel is null)
            {
                return; 
            }

            ViewModel.OpenFileInteraction.RegisterHandler(ShowOpenFileDialog).DisposeWith(d);
            ViewModel.SaveAsInteraction.RegisterHandler(ShowSaveAsDialog).DisposeWith(d);
        });
    }

    private async Task ShowSaveAsDialog(IInteractionContext<Unit, string?> arg)
    {
        // TODO
        var result = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions());
    }

    private async Task ShowOpenFileDialog(IInteractionContext<Unit, string?> ctx)
    {
        var result = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Select .far file",
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType(".far") { Patterns = [".far"] }],
            }
        );
        ctx.SetOutput(result[0].TryGetLocalPath());
    }

    private void MenuItemExit_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
