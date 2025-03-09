using ReactiveUI;
using Sims.Far;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Models;

public class FarVm(Far f) : ReactiveObject
{
    public FarVersion Version
    {
        get => _version;
        set => this.RaiseAndSetIfChanged(ref _version, value);
    }

    public List<FarFileVm> Files => f.Files.Select(x => new FarFileVm(x)).ToList();

    private FarVersion _version = f.Version;
}
