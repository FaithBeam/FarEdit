using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;
using ReactiveUI;

namespace FarEdit.Core.ViewModels.MainWindowViewModel;

public class MainWindowViewModel : ViewModelBase
{
    public bool IsImage => _isImage.Value;
    public string? FarPath => _farPath?.Value;

    public GetFarFiles.FarFileVm? SelectedFarFileVm
    {
        get => _selectedFarFileVm;
        set => this.RaiseAndSetIfChanged(ref _selectedFarFileVm, value);
    }

    public string? EntryFilter
    {
        get => _entryFilter;
        set => this.RaiseAndSetIfChanged(ref _entryFilter, value);
    }

    public ReadOnlyObservableCollection<GetFarFiles.FarFileVm> FarFiles => _farFiles;

    public ReactiveCommand<Unit, string?> OpenFileCmd { get; }
    public IInteraction<Unit, string?> OpenFileInteraction { get; }

    public MainWindowViewModel(GetFarFiles.Handler getFarFilesHandler)
    {
        var dynamicEntryFilter = this.WhenAnyValue(x => x.EntryFilter)
            .Select(CreateEntryFilterPredicate);
        var entrySc = new SourceCache<GetFarFiles.FarFileVm, string>(x => x.Name);
        entrySc
            .Connect()
            .Filter(dynamicEntryFilter)
            .SortAndBind(
                out _farFiles,
                SortExpressionComparer<GetFarFiles.FarFileVm>.Ascending(x => x.Name)
            )
            .Subscribe();
        this.WhenAnyValue(x => x.FarPath)
            .WhereNotNull()
            .Where(File.Exists)
            .Select(x => getFarFilesHandler.Execute(new GetFarFiles.Query(x)))
            .Subscribe(x =>
                entrySc.Edit(inner =>
                {
                    inner.Clear();
                    inner.AddOrUpdate(x.Files);
                })
            );
        _isImage = this.WhenAnyValue(x => x.SelectedFarFileVm)
            .WhereNotNull()
            .Select(x => x.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
            .ToProperty(this, x => x.IsImage);

        OpenFileInteraction = new Interaction<Unit, string?>();
        OpenFileCmd = ReactiveCommand.CreateFromTask(
            async () => await OpenFileInteraction.Handle(Unit.Default)
        );
        _farPath = OpenFileCmd.ToProperty(this, x => x.FarPath);
    }

    private static Func<GetFarFiles.FarFileVm, bool> CreateEntryFilterPredicate(string? txt)
    {
        if (string.IsNullOrWhiteSpace(txt))
        {
            return _ => true;
        }

        return ffVm => ffVm.Name.Contains(txt, StringComparison.OrdinalIgnoreCase);
    }

    private readonly ObservableAsPropertyHelper<bool> _isImage;
    private readonly ReadOnlyObservableCollection<GetFarFiles.FarFileVm> _farFiles;
    private string? _entryFilter;
    private readonly ObservableAsPropertyHelper<string?>? _farPath;
    private GetFarFiles.FarFileVm? _selectedFarFileVm;
}
