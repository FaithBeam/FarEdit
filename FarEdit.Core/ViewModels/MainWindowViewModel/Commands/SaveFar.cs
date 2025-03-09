using FarEdit.Core.ViewModels.MainWindowViewModel.Models;
using Sims.Far;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Commands;

public static class SaveFar
{
    public sealed record Command(string Path, IEnumerable<FarFileVm> Files, FarVersion Version);

    public sealed class Handler
    {
        public void Execute(Command c)
        {
            var far = new Far(c.Version, c.Files.Select(x => new FarFile(x.Name, x.Data)).ToList());
            far.Write(c.Path);
        }
    }
}
