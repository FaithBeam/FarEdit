using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;
using Sims.Far;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Commands;

public static class Save
{
    public sealed record Command(string Path, IEnumerable<GetFarFiles.FarFileVm> Files);

    public sealed class Handler
    {
        public void Execute(Command c)
        {
            var far = new Far(c.Files.Select(x => new FarFile(x.Name, x.Data)).ToList());
            far.Write(c.Path);
        }
    }
}
