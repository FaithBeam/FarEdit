using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using FarEdit.Core.ViewModels.Dialogs.YesNoDialog;
using FarEdit.Core.ViewModels.MainWindowViewModel.Commands;
using FarEdit.Core.ViewModels.MainWindowViewModel.Models;
using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;
using ReactiveUI;

namespace FarEdit.Core.ViewModels.MainWindowViewModel;

public class MainWindowViewModel : ViewModelBase
{
    public bool IsImage => _isImage.Value;
    public string? FarPath => _farPath?.Value;

    public FarFileVm? SelectedFarFileVm => _selectedFarFileVm.Value;

    public ObservableCollection<FarFileVm>? SelectedFarFiles { get; set; }

    public string? EntryFilter
    {
        get => _entryFilter;
        set => this.RaiseAndSetIfChanged(ref _entryFilter, value);
    }

    public ReadOnlyObservableCollection<FarFileVm> FarFiles => _farFiles;

    public ReadOnlyObservableCollection<FarFileVm> UnFilteredFarFiles => _unFilteredFarFiles;

    public ReactiveCommand<Unit, string?> OpenFileCmd { get; }
    public IInteraction<Unit, string?> OpenFileInteraction { get; }
    public ReactiveCommand<
        (string, ReadOnlyObservableCollection<FarFileVm>),
        Unit
    > SaveCommand { get; }
    public ReactiveCommand<
        (string, ReadOnlyObservableCollection<FarFileVm>),
        string?
    > SaveAsCommand { get; }
    public IInteraction<string, string?> SaveAsInteraction { get; }
    public ReactiveCommand<IList<FarFileVm>, Unit> ExportCommand { get; }
    public IInteraction<Unit, string?> ExportInteraction { get; }
    public ReactiveCommand<Unit, List<FarFileVm>> AddEntriesCommand { get; }
    public IInteraction<Unit, List<string>> AddEntriesInteraction { get; }
    public ReactiveCommand<IList<FarFileVm>, Unit> RemoveEntriesCmd { get; }
    public IInteraction<Unit, YesNoDialogResponse> RemoveEntriesInteraction { get; }
    public ReactiveCommand<Unit, string?> NewFileCmd { get; }
    public IInteraction<Unit, string?> NewFileInteraction { get; }

    public MainWindowViewModel(
        GetFarVm.Handler getFarFilesHandler,
        SaveFar.Handler saveHandler,
        ExportEntries.Handler exportHandler,
        GetFarFilesFromPaths.Handler getFarFilesFromPathsHandler
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
                        (ObservableCollection<FarFileVm>?)x.Sender
                    )?.LastOrDefault(),
                    NotifyCollectionChangedAction.Add => x.EventArgs.NewItems?[0] as FarFileVm,
                    NotifyCollectionChangedAction.Replace => x.EventArgs.NewItems?[0] as FarFileVm,
                    NotifyCollectionChangedAction.Move => x.EventArgs.NewItems?[0] as FarFileVm,
                    NotifyCollectionChangedAction.Reset => x.EventArgs.NewItems?[0] as FarFileVm,
                    _ => throw new ArgumentOutOfRangeException(),
                }
            )
            .ToProperty(this, x => x.SelectedFarFileVm);
        var dynamicEntryFilter = this.WhenAnyValue(x => x.EntryFilter)
            .Select(CreateEntryFilterPredicate);
        var entrySc = new SourceCache<FarFileVm, string>(x => x.Name);
        entrySc.Connect().Bind(out _unFilteredFarFiles).Subscribe();
        entrySc
            .Connect()
            .Filter(dynamicEntryFilter)
            .SortAndBind(out _farFiles, SortExpressionComparer<FarFileVm>.Ascending(x => x.Name))
            .Subscribe();
        this.WhenAnyValue(x => x.FarPath)
            .WhereNotNull()
            .Where(File.Exists)
            .Select(x => getFarFilesHandler.Execute(new GetFarVm.Query(x)))
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
            x => x.FarPath,
            selector: farPath => !string.IsNullOrWhiteSpace(farPath)
        );
        SaveCommand = ReactiveCommand.Create<(string, ReadOnlyObservableCollection<FarFileVm>)>(
            tuple => saveHandler.Execute(new SaveFar.Command(tuple.Item1, tuple.Item2)),
            canSave
        );

        SaveAsInteraction = new Interaction<string, string?>();
        var canSaveAs = this.WhenAnyValue(
            x => x.FarPath,
            selector: farPath => !string.IsNullOrWhiteSpace(farPath)
        );
        SaveAsCommand = ReactiveCommand.CreateFromTask<
            (string, ReadOnlyObservableCollection<FarFileVm>),
            string?
        >(
            async tuple =>
            {
                var dst = await SaveAsInteraction.Handle(Path.GetExtension(tuple.Item1));
                if (string.IsNullOrWhiteSpace(dst))
                {
                    return null;
                }
                saveHandler.Execute(new SaveFar.Command(dst, tuple.Item2));
                return dst;
            },
            canSaveAs
        );

        ExportInteraction = new Interaction<Unit, string?>();
        ExportCommand = ReactiveCommand.CreateFromTask<IList<FarFileVm>>(async selectedEntries =>
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

            await exportHandler.Execute(new ExportEntries.Command(dstFolder, selectedEntries));
        });

        AddEntriesInteraction = new Interaction<Unit, List<string>>();
        var canAddEntries = this.WhenAnyValue(
            x => x.FarPath,
            selector: farPath => !string.IsNullOrWhiteSpace(farPath)
        );
        AddEntriesCommand = ReactiveCommand.CreateFromTask<List<FarFileVm>>(
            async () =>
            {
                var paths = await AddEntriesInteraction.Handle(Unit.Default);
                return getFarFilesFromPathsHandler.Execute(new GetFarFilesFromPaths.Query(paths));
            },
            canAddEntries
        );
        AddEntriesCommand.Subscribe(x => entrySc.AddOrUpdate(x));

        RemoveEntriesInteraction = new Interaction<Unit, YesNoDialogResponse>();
        RemoveEntriesCmd = ReactiveCommand.CreateFromTask<IList<FarFileVm>>(async entriesToRemove =>
        {
            var response = await RemoveEntriesInteraction.Handle(Unit.Default);
            if (response.Result)
            {
                entrySc.Remove(entriesToRemove);
            }
        });

        NewFileInteraction = new Interaction<Unit, string?>();
        NewFileCmd = ReactiveCommand.CreateFromTask<string?>(
            async () => await NewFileInteraction.Handle(Unit.Default)
        );
        NewFileCmd.Subscribe(_ => entrySc.Clear());

        _farPath = OpenFileCmd
            .Merge(SaveAsCommand)
            .Merge(NewFileCmd)
            .ToProperty(this, x => x.FarPath);
    }

    private static Func<FarFileVm, bool> CreateEntryFilterPredicate(string? txt) =>
        string.IsNullOrWhiteSpace(txt)
            ? _ => true
            : ffVm => ffVm.Name.Contains(txt, StringComparison.OrdinalIgnoreCase);

    private readonly ObservableAsPropertyHelper<bool> _isImage;
    private readonly ReadOnlyObservableCollection<FarFileVm> _farFiles;
    private readonly ReadOnlyObservableCollection<FarFileVm> _unFilteredFarFiles;

    private string? _entryFilter;
    private readonly ObservableAsPropertyHelper<string?>? _farPath;
    private readonly ObservableAsPropertyHelper<FarFileVm?> _selectedFarFileVm;
}
