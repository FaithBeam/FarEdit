using Sims.Far;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Models;

public class FarVm(Far f)
{
    public List<FarFileVm> Files => f.Files.Select(x => new FarFileVm(x)).ToList();
}
