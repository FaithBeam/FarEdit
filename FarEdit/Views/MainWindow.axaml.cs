using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
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
            ViewModel.ExportInteraction.RegisterHandler(ShowExportDialog).DisposeWith(d);
        });
    }

    private async Task ShowExportDialog(IInteractionContext<Unit, string?> ctx)
    {
        var result = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = "Select a folder" }
        );
        ctx.SetOutput(result[0].TryGetLocalPath());
    }

    private async Task ShowSaveAsDialog(IInteractionContext<string, string?> ctx)
    {
        var result = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save As",
                FileTypeChoices =
                [
                    new FilePickerFileType(ctx.Input)
                    {
                        Patterns = [System.IO.Path.GetExtension(ctx.Input)],
                    },
                ],
            }
        );
        ctx.SetOutput(result?.TryGetLocalPath());
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
