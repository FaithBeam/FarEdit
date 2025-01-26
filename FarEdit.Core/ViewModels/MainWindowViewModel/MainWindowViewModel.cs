using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using FarEdit.Core.ViewModels.MainWindowViewModel.Commands;
using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;
using ReactiveUI;

namespace FarEdit.Core.ViewModels.MainWindowViewModel;

public class MainWindowViewModel : ViewModelBase
{
    public bool IsImage => _isImage.Value;
    public string? FarPath => _farPath?.Value;

    public GetFarFiles.FarFileVm? SelectedFarFileVm => _selectedFarFileVm.Value;

    public ObservableCollection<GetFarFiles.FarFileVm>? SelectedFarFiles { get; }

    public string? EntryFilter
    {
        get => _entryFilter;
        set => this.RaiseAndSetIfChanged(ref _entryFilter, value);
    }

    public ReadOnlyObservableCollection<GetFarFiles.FarFileVm> FarFiles => _farFiles;

    public ReadOnlyObservableCollection<GetFarFiles.FarFileVm> UnFilteredFarFiles =>
        _unFilteredFarFiles;

    public ReactiveCommand<Unit, string?> OpenFileCmd { get; }
    public IInteraction<Unit, string?> OpenFileInteraction { get; }
    public ReactiveCommand<
        (string, ReadOnlyObservableCollection<GetFarFiles.FarFileVm>),
        Unit
    > SaveCommand { get; }
    public ReactiveCommand<
        (string, ReadOnlyObservableCollection<GetFarFiles.FarFileVm>),
        string?
    > SaveAsCommand { get; }
    public IInteraction<string, string?> SaveAsInteraction { get; }
    public ReactiveCommand<IList<GetFarFiles.FarFileVm>, Unit> ExportCommand { get; }
    public IInteraction<Unit, string?> ExportInteraction { get; }

    public MainWindowViewModel(
        GetFarFiles.Handler getFarFilesHandler,
        Save.Handler saveHandler,
        Export.Handler exportHandler
    )
    {
        SelectedFarFiles = [];
        var selectedFarFilesChangedObs = Observable.FromEventPattern<
            NotifyCollectionChangedEventHandler,
            NotifyCollectionChangedEventArgs
        >(
            handler => SelectedFarFiles.CollectionChanged += handler,
            handler => SelectedFarFiles.CollectionChanged -= handler
        );
        _selectedFarFileVm = selectedFarFilesChangedObs
            .Select(x =>
                x.EventArgs.Action switch
                {
                    NotifyCollectionChangedAction.Remove => (
                        (ObservableCollection<GetFarFiles.FarFileVm>?)x.Sender
                    )?.LastOrDefault(),
                    NotifyCollectionChangedAction.Add => x.EventArgs.NewItems?[0]
                        as GetFarFiles.FarFileVm,
                    NotifyCollectionChangedAction.Replace => x.EventArgs.NewItems?[0]
                        as GetFarFiles.FarFileVm,
                    NotifyCollectionChangedAction.Move => x.EventArgs.NewItems?[0]
                        as GetFarFiles.FarFileVm,
                    NotifyCollectionChangedAction.Reset => x.EventArgs.NewItems?[0]
                        as GetFarFiles.FarFileVm,
                    _ => throw new ArgumentOutOfRangeException(),
                }
            )
            .ToProperty(this, x => x.SelectedFarFileVm);
        var dynamicEntryFilter = this.WhenAnyValue(x => x.EntryFilter)
            .Select(CreateEntryFilterPredicate);
        var entrySc = new SourceCache<GetFarFiles.FarFileVm, string>(x => x.Name);
        entrySc.Connect().Bind(out _unFilteredFarFiles).Subscribe();
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
            .Select(x =>
                x.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                || x.Name.EndsWith(".tga", StringComparison.OrdinalIgnoreCase)
            )
            .ToProperty(this, x => x.IsImage);

        OpenFileInteraction = new Interaction<Unit, string?>();
        // <string?> is necessary even though it says it isn't
        OpenFileCmd = ReactiveCommand.CreateFromTask<string?>(
            async () => await OpenFileInteraction.Handle(Unit.Default)
        );

        var canSave = this.WhenAnyValue(
            x => x.FarFiles,
            property2: x => x.FarPath,
            selector: (farFiles, farPath) => farFiles.Any() && !string.IsNullOrWhiteSpace(farPath)
        );
        SaveCommand = ReactiveCommand.Create<(
            string,
            ReadOnlyObservableCollection<GetFarFiles.FarFileVm>
        )>(tuple => saveHandler.Execute(new Save.Command(tuple.Item1, tuple.Item2)), canSave);

        SaveAsInteraction = new Interaction<string, string?>();
        var canSaveAs = this.WhenAnyValue(
            x => x.FarPath,
            selector: farPath => !string.IsNullOrWhiteSpace(farPath)
        );
        SaveAsCommand = ReactiveCommand.CreateFromTask<
            (string, ReadOnlyObservableCollection<GetFarFiles.FarFileVm>),
            string?
        >(
            async tuple =>
            {
                var dst = await SaveAsInteraction.Handle(Path.GetExtension(tuple.Item1));
                if (string.IsNullOrWhiteSpace(dst))
                {
                    return null;
                }
                saveHandler.Execute(new Save.Command(dst, tuple.Item2));
                return dst;
            },
            canSaveAs
        );
        _farPath = OpenFileCmd.Merge(SaveAsCommand).ToProperty(this, x => x.FarPath);

        ExportInteraction = new Interaction<Unit, string?>();
        ExportCommand = ReactiveCommand.CreateFromTask<IList<GetFarFiles.FarFileVm>>(
            async selectedEntries =>
            {
                if (selectedEntries.Count == 0)
                {
                    return;
                }
                var dstFolder = await ExportInteraction.Handle(Unit.Default);
                if (string.IsNullOrWhiteSpace(dstFolder))
                {
                    return;
                }

                await exportHandler.Execute(new Export.Command(dstFolder, selectedEntries));
            }
        );
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
    private readonly ReadOnlyObservableCollection<GetFarFiles.FarFileVm> _unFilteredFarFiles;

    private string? _entryFilter;
    private readonly ObservableAsPropertyHelper<string?>? _farPath;
    private readonly ObservableAsPropertyHelper<GetFarFiles.FarFileVm?> _selectedFarFileVm;
    // private ObservableCollection<GetFarFiles.FarFileVm>? _selectedFarFiles;
}
