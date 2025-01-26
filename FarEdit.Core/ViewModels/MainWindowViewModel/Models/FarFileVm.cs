using ReactiveUI;
using Sims.Far;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Models;

public class FarFileVm(FarFile f) : ReactiveObject
{
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    public byte[] Data
    {
        get => _data;
        set => this.RaiseAndSetIfChanged(ref _data, value);
    }

    private string _name = f.Name;
    private byte[] _data = f.Bytes;
}
