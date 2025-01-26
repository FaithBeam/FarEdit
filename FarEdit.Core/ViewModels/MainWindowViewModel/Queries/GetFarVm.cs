using FarEdit.Core.ViewModels.MainWindowViewModel.Models;
using Sims.Far;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Queries;

public static class GetFarVm
{
    public sealed record Query(string PathToFar);

    public sealed class Handler
    {
        public FarVm Execute(Query query) => new(Far.Read(query.PathToFar));
    }
}
