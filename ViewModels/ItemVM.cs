using System.Windows.Media;

namespace lab4_extra.ViewModels;

public class ItemVM : BaseVM
{
    private string _value = "";

    private Color _color = Colors.White;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            OnPropertyChanged();
        }
    }
    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged();
        }
    }

    public ItemVM(string value)
    {
        Value = value;
    }

    public ItemVM Clone() {
        var clone = new ItemVM(Value);
        clone.Color = Color;
        return clone;
    }
}