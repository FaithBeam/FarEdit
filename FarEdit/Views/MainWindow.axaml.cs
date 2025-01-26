using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using DynamicData;
using FarEdit.Core.ViewModels.MainWindowViewModel;
using FarEdit.Core.ViewModels.MainWindowViewModel.Models;
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
            ViewModel.AddEntriesInteraction.RegisterHandler(ShowAddEntriesDialog).DisposeWith(d);
        });
    }

    private async Task ShowAddEntriesDialog(IInteractionContext<Unit, List<string>> ctx)
    {
        var paths = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { Title = "Select files to add", AllowMultiple = true }
        );
        ctx.SetOutput(
            paths
                .Select(x => x.TryGetLocalPath() ?? "")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList()
        );
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

    private void FarFileDataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dg)
        {
            return;
        }

        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        vm.SelectedFarFiles?.Clear();
        vm.SelectedFarFiles?.AddRange(dg.SelectedItems.Cast<FarFileVm>());
    }
}
