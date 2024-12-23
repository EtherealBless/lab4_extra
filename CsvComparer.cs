namespace lab4_extra;

class CsvComparer : IComparer<string>
{
    private readonly int _index;
    private readonly bool _asDouble;
    private readonly char _separator;
    public CsvComparer(int index, bool asDouble, char separator)
    {
        _index = index;
        _asDouble = asDouble;
        _separator = separator;
    }
    public int Compare(string? x, string? y)
    {
        if (_asDouble)
        {
            return double.Parse(x?.Split(_separator)[_index]) - double.Parse(y?.Split(_separator)[_index]) > 0 ? 1 : -1;
        }
        else
        {
            if (x == "")
            {
                return -1;
            }
            if (y == "")
            {
                return 1;
            }
            return x?.Split(_separator)[_index].CompareTo(y?.Split(_separator)[_index]) ?? 0;
        }
    }
}