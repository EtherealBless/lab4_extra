using System.Diagnostics;

namespace lab4_extra.ViewModels;

public class FileVM : BaseVM
{
    private ItemVM[] _items = Array.Empty<ItemVM>();
    public ItemVM[] Items { get => _items; set { _items = value; OnPropertyChanged();  } }

    public FileVM Clone() => new FileVM { Items = Items.Select(x => x.Clone()).ToArray() };
}