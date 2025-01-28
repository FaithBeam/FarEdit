using System;
using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using FarEdit.Core.ViewModels.Dialogs.YesNoDialog;
using ReactiveUI;

namespace FarEdit.Dialogs;

public partial class CustomYesNoDialog : ReactiveWindow<CustomYesNoDialogViewModel>
{
    public CustomYesNoDialog()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            if (ViewModel is null)
            {
                return;
            }
            ViewModel.YesCommand.Subscribe(Close).DisposeWith(d);
            ViewModel.NoCommand.Subscribe(Close).DisposeWith(d);
        });
    }
}
