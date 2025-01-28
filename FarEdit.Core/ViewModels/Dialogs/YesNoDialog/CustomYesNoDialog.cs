using System.Reactive;
using ReactiveUI;

namespace FarEdit.Core.ViewModels.Dialogs.YesNoDialog;

public class CustomYesNoDialogViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, YesNoDialogResponse> YesCommand { get; } =
        ReactiveCommand.Create(() => new YesNoDialogResponse { Result = true });
    public ReactiveCommand<Unit, YesNoDialogResponse> NoCommand { get; } =
        ReactiveCommand.Create(() => new YesNoDialogResponse { Result = false });

    public string Title { get; set; } = "Default Title";
    public string Message { get; set; } = "Default message.";
}

public record YesNoDialogResponse
{
    public bool Result { get; set; }
}
