using FarEdit.Core.ViewModels.MainWindowViewModel.Models;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Commands;

public static class ExportEntries
{
    public sealed record Command(string Path, IList<FarFileVm> Vms);

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
