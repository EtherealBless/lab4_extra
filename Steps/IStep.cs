using System.ComponentModel;
using System.Formats.Asn1;
using System.Windows.Media;

namespace lab4_extra.Steps;

public interface IStep
{
}

public class CompareStep : IStep
{
    /// <summary>
    /// Indecies of file and file item that should be compared
    /// </summary>
    public List<(int, int)> Comparisons { get; set; }

    public CompareStep(List<(int, int)> comparisons)
    {
        Comparisons = comparisons;
    }

}
public class ResetRunColorsStep : IStep
{
    public ResetRunColorsStep()
    {
    }
}

public class MoveElementStep : IStep
{
    public int SourceFileIndex { get; set; }
    public int TargetFileIndex { get; set; }
    public int SourceFileItemIndex { get; set; }
    public int TargetFileItemIndex { get; set; }

}

public class MoveRangeStep : IStep
{
    public int SourceFileIndex { get; set; }
    public int TargetFileIndex { get; set; }
    public int SourceFileItemStartIndex { get; set; }
    public int SourceFileItemEndIndex { get; set; }
    public int TargetFileItemStartIndex { get; set; }
    public int TargetFileItemEndIndex { get; set; }
}

public class RawStep : IStep
{ }

public class RawColorStep : RawStep
{
    public int FileIndex { get; set; }
    public int FileItemIndex { get; set; }
    public Color Color { get; set; }

    public RawColorStep(int fileIndex, int fileItemIndex, Color color)
    {
        FileIndex = fileIndex;
        FileItemIndex = fileItemIndex;
        Color = color;
    }
}

public class RawMoveStep : RawStep
{
    public int SourceFileIndex { get; set; }
    public int TargetFileIndex { get; set; }
    public int SourceFileItemIndex { get; set; }
    public int TargetFileItemIndex { get; set; }

    public RawMoveStep(int sourceFileIndex, int sourceFileItemIndex, int targetFileIndex, int targetFileItemIndex)
    {
        SourceFileIndex = sourceFileIndex;
        TargetFileIndex = targetFileIndex;
        SourceFileItemIndex = sourceFileItemIndex;
        TargetFileItemIndex = targetFileItemIndex;
    }
}

public class CommentStep : IStep
{
    public string Comment { get; set; }
    public CommentStep(string comment)
    {
        Comment = comment;
    }
}

public class MarkRunStep : IStep
{
    public int FileIndex { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    private static int _colorIndex = 0;
    public Color Color => Constants.ColorsConstants.RunColorList[_colorIndex++ % Constants.ColorsConstants.RunColorList.Count];

    public MarkRunStep(int fileIndex, int startIndex, int endIndex)
    {
        FileIndex = fileIndex;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }
    public static void ResetColorIndex() => _colorIndex = 0;
}

public class MarkAllRunsStep : IStep
{
    public List<Run> Runs { get; set; }
    private int _colorIndex = 0;
    public Color Color => Constants.ColorsConstants.RunColorList[_colorIndex++ % Constants.ColorsConstants.RunColorList.Count];

    public MarkAllRunsStep(List<Run> runs)
    {
        Runs = runs;
    }
}

public class FlushStep : RawStep
{
}
