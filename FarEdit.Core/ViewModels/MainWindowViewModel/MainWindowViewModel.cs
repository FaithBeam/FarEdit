using System.Reactive.Linq;
using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;
using ReactiveUI;

namespace FarEdit.Core.ViewModels.MainWindowViewModel;

public class MainWindowViewModel : ViewModelBase
{
    public string? FarPath
    {
        get => _farPath;
        set => this.RaiseAndSetIfChanged(ref _farPath, value);
    }

    public GetFarFiles.FarVm? FarVm => _farVm?.Value;

    public GetFarFiles.FarFileVm? SelectedFarFileVm
    {
        get => _selectedFarFileVm;
        set => this.RaiseAndSetIfChanged(ref _selectedFarFileVm, value);
    }

    public MainWindowViewModel(GetFarFiles.Handler getFarFilesHandler)
    {
        _farVm = this.WhenAnyValue(x => x.FarPath)
            .WhereNotNull()
            .Where(File.Exists)
            .Select(x => getFarFilesHandler.Execute(new GetFarFiles.Query(x)))
            .ToProperty(this, x => x.FarVm);
    }

    private string? _farPath;
    private readonly ObservableAsPropertyHelper<GetFarFiles.FarVm?>? _farVm;
    private GetFarFiles.FarFileVm? _selectedFarFileVm;
}
