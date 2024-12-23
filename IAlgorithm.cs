using System.Printing;
using lab4_extra.Steps;

namespace lab4_extra;

public interface IAlgorithm
{
    public int Files { get; }
    public IEnumerable<IStep> Run();
/// <summary>
/// Loads data from the specified input file path.
/// </summary>
/// <param name="inputPath">The path to the input file containing the data to be loaded.</param>

    protected virtual void LoadData(string inputPath) { 

    }
}