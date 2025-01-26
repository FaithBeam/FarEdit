using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;

namespace FarEdit.Core.ViewModels.MainWindowViewModel.Commands;

public static class Export
{
    public sealed record Command(string Path, GetFarFiles.FarFileVm Vm);

    public sealed class Handler
    {
        public async Task Execute(Command c) => await File.WriteAllBytesAsync(c.Path, c.Vm.Data);
    }
}
