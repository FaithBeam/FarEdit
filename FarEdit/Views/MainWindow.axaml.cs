using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using DynamicData;
using FarEdit.Core.ViewModels.Dialogs.YesNoDialog;
using FarEdit.Core.ViewModels.MainWindowViewModel;
using FarEdit.Core.ViewModels.MainWindowViewModel.Models;
using FarEdit.Dialogs;
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
            ViewModel.NewFileInteraction.RegisterHandler(ShowNewFileDialog).DisposeWith(d);
            ViewModel.RemoveEntriesInteraction.RegisterHandler(RemoveEntriesDialog).DisposeWith(d);
        });
    }

    private async Task RemoveEntriesDialog(IInteractionContext<Unit, YesNoDialogResponse> ctx)
    {
        var customYesNoDialog = new CustomYesNoDialog
        {
            DataContext = new CustomYesNoDialogViewModel
            {
                Title = "Remove Entries",
                Message = "Are you sure you want to remove these entries?",
            },
        };
        var result =
            await customYesNoDialog.ShowDialog<YesNoDialogResponse?>(this)
            ?? new YesNoDialogResponse { Result = false };
        ctx.SetOutput(result);
    }

    private async Task ShowNewFileDialog(IInteractionContext<Unit, string?> ctx)
    {
        var result = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "New File",
                FileTypeChoices = [new FilePickerFileType("*.far") { Patterns = ["*.far"] }],
            }
        );
        ctx.SetOutput(result?.TryGetLocalPath());
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
            new FolderPickerOpenOptions { Title = "Export Entries to Folder" }
        );
        ctx.SetOutput(result.Any() ? result[0].TryGetLocalPath() : string.Empty);
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
        ctx.SetOutput(result.Any() ? result[0].TryGetLocalPath() : string.Empty);
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

    private async Task ShowCustomYesNoDialogAsync(
        IInteractionContext<CustomYesNoDialogViewModel?, YesNoDialogResponse?> interaction
    )
    {
        var dialog = new CustomYesNoDialog { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<YesNoDialogResponse?>(this);
        interaction.SetOutput(result);
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e)
        {
            case { Key: Key.O, KeyModifiers: KeyModifiers.Control }:
                OpenMenuItem.Command?.Execute(Unit.Default);
                break;
            case { Key: Key.S, KeyModifiers: KeyModifiers.Control }:
                SaveBtn.Command?.Execute((ViewModel?.FarPath, ViewModel?.UnFilteredFarFiles));
                break;
            case { Key: Key.N, KeyModifiers: KeyModifiers.Control }:
                NewFarFileBtn.Command?.Execute(Unit.Default);
                break;
            case { Key: Key.E, KeyModifiers: KeyModifiers.Control }:
                ExportEntriesBtn.Command?.Execute(ViewModel?.SelectedFarFiles);
                break;
            case { Key: Key.Delete, KeyModifiers: KeyModifiers.Control }:
                DeleteEntriesBtn.Command?.Execute(ViewModel?.SelectedFarFiles);
                break;
        }
    }
}
