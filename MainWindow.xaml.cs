using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using lab4_extra.ViewModels;

namespace lab4_extra;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private WindowVM _windowVM;
    public WindowVM WindowVM
    {
        get => _windowVM;
        set
        {
            _windowVM = value;
            DataContext = _windowVM;
        }
    }


    public MainWindow()
    {
        // var testSort = new NWayMergeSort<string>(3, new CsvComparer(1, false, ','));
        // testSort.Sort("input.txt", "output.txt");
        InitializeComponent();
        WindowVM = new WindowVM();
        // _timer.Start();
    }
}