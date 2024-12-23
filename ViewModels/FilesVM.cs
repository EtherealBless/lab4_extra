using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Documents;
using lab4_extra.Steps;

namespace lab4_extra.ViewModels;

public class FilesVM : BaseVM
{
    public ObservableCollection<string> StepsDescription { get; private set; } = new();

    private FileVM[] _files;

    public FileVM[] Files
    {
        get
        {
            // Debug.WriteLine($"Files get: {_files.Length}");
            if (_files[0] == null)
            {
                return _files;
            }
            // for (int i = 0; i < _files.Length; i++)
            // {
            //     for (int j = 0; j < (_files[i]?.Items?.Length ?? 0); j++)
            //     {
            //         Debug.WriteLine($"File {i} item {j}: {_files[i]?.Items[j]?.Value} {_files[i]?.Items[j]?.Color}");
            //     }
            // }
            return _files;
        }
        set
        {
            _files = value;
            OnPropertyChanged();
        }
    }

    public FilesVM(int files)
    {
        Files = new FileVM[files];
    }

    private FilesVM() { }

    public FilesVM Clone()
    {
        var clone = new FilesVM(Files.Length);
        clone.Files = Files.Select(x => x.Clone()).ToArray();
        clone.StepsDescription = new ObservableCollection<string>(StepsDescription);
        return clone;
    }
}