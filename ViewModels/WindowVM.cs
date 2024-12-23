using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace lab4_extra.ViewModels;

public class WindowVM : BaseVM
{

    public FilesVM FilesVM { get => _filesVM; set { _filesVM = value; OnPropertyChanged(); } }
    private FilesVM _filesVM;

    private DispatcherTimer _timer;
    CsvComparer _comparer;
    private bool _isRunning = false;
    private VisualizationManager _visualizationManager;
    private string _inputPath = "input.txt";
    private Queue<FileVM> _statesQueue = new();
    public RelayCommand StartCommand => new(Start, CanRun);
    public RelayCommand StopCommand => new(o => { _timer.Stop(); _isRunning = false; }, o => _isRunning);
    public RelayCommand StepForwardCommand => new(StepForward, o => !_isRunning && (_visualizationManager?.CanStepForward ?? false));
    public RelayCommand StepBackwardCommand => new(StepBack, o => !_isRunning && (_visualizationManager?.CanStepBack ?? false));
    public Dictionary<string, IAlgorithm> Algorithms { get; private set; }
    public IAlgorithm SelectedAlgorithm
    {
        get => _algorithm;
        set
        {
            _algorithm = value;
            OnPropertyChanged();
        }
    }
    private IAlgorithm _algorithm;

    private bool CanRun(object o) => !_isRunning;
    private void Start(object o)
    {
        _visualizationManager = new VisualizationManager(SelectedAlgorithm, FilesVM);
        _isRunning = true;
        _visualizationManager.LoadData(_inputPath, 1);
        _visualizationManager.WarmUp();
        _timer.Start();
    }
    private void StepBack(object o = null!)
    {
        FilesVM = _visualizationManager.StepBack();
        OnPropertyChanged(nameof(FilesVM));
    }
    private void StepForward(object o = null!)
    {
        FilesVM = _visualizationManager.StepForward();
        OnPropertyChanged(nameof(FilesVM));
    }
    int temp = 0;
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (!_visualizationManager.CanStepForward) return;

        StepForward();

        // Thread.Sleep(100);
        // Debug.WriteLine(temp++);
    }

    public WindowVM()
    {
        FilesVM = new FilesVM(3);
        TestInit();
        _comparer = new(1, false, ',');
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(50);
        _timer.Tick += Timer_Tick;
        Algorithms = new()
        {
            { "TestAlgo", new TestAlgo("input.txt", "output.txt", _comparer) },
            { "TestAlgoNatural", new TestAlgoNatural("input.txt", "output.txt", _comparer) },
        };
    }

    private void TestInit()
    {
        FilesVM = new FilesVM(3);
        FilesVM.Files[0] = new FileVM();
        FilesVM.Files[1] = new FileVM();
        FilesVM.Files[2] = new FileVM();

        FilesVM.Files[0].Items = [new ItemVM("1"), new ItemVM("2"), new ItemVM("3")];
        FilesVM.Files[1].Items = [new ItemVM("4"), new ItemVM("5"), new ItemVM("6")];
        FilesVM.Files[2].Items = [new ItemVM("7"), new ItemVM("8"), new ItemVM("9")];
    }
}