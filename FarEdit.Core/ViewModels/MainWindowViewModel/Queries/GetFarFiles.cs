using Sims.Far;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Queries;

public static class GetFarFiles
{
    public sealed record Query(string PathToFar);

    public class FarVm(Far f)
    {
        public List<FarFileVm> Files => f.Files.Select(x => new FarFileVm(x)).ToList();
    }

    public class FarFileVm(FarFile f)
    {
        public string Name => f.Name;
        public byte[] Data => f.Bytes;
    }

    public sealed class Handler
    {
        public FarVm Execute(Query query) => new(Far.Read(query.PathToFar));
    }
}