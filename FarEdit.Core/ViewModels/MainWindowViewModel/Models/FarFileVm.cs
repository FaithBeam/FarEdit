using Sims.Far;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Models;

public class FarFileVm(FarFile f)
{
    public string Name => f.Name;
    public byte[] Data => f.Bytes;
}
