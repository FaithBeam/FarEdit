using FarEdit.Core.ViewModels.MainWindowViewModel.Models;
using Sims.Far;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Queries;

public static class GetFarFilesFromPaths
{
    public sealed record Query(IEnumerable<string> Paths);

    public sealed class Handler
    {
        public List<FarFileVm> Execute(Query q) =>
            q
                .Paths.Select(x => new FarFileVm(
                    new FarFile(Path.GetFileName(x), File.ReadAllBytes(x))
                ))
                .ToList();
    }
}
