using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Commands;

public static class Export
{
    public sealed record Command(string Path, IList<GetFarFiles.FarFileVm> Vms);

    public sealed class Handler
    {
        public async Task Execute(Command c)
        {
            foreach (var f in c.Vms)
            {
                await File.WriteAllBytesAsync(
                    Path.Join(
                        c.Path,
                        Path.GetFileName(f.Name.Replace('\\', Path.DirectorySeparatorChar))
                    ),
                    f.Data
                );
            }
        }
    }
}
