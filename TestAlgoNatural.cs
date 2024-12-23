using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Automation;
using lab4_extra.Steps;

namespace lab4_extra;

//example external merge sort
public class TestAlgoNatural : IAlgorithm
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private readonly IComparer<string> _comparer;
    private bool _canMerge = true;
    private FileStream[] _files;
    public int Files { get => 3; }
    int _lines = 0;

    public TestAlgoNatural(string inputPath, string outputPath, IComparer<string> comparer)
    {
        _inputPath = inputPath;
        _outputPath = outputPath;
        _comparer = comparer;
    }

    private IEnumerable<IStep> FindRuns(int inputFileIndex)
    {
        var files = OpenFiles(rewriteTempFiles: false);
        var runsLength = GetRunsLength(files, inputFileIndex)[inputFileIndex];
        var file = files[inputFileIndex];
        var currentRun = 0;
        var runStartIndex = 0;
        var runEndIndex = runsLength - 1;
        while (runsLength > 0)
        {
            // var file = new StreamReader(_files[inputFileIndex]);
            while (ReadLine(file.Item1.BaseStream, true) is string line)
            {
                if (string.IsNullOrEmpty(line)) break;
                if (currentRun == runsLength)
                {
                    yield return new MarkRunStep(inputFileIndex, runStartIndex, runEndIndex);
                    runStartIndex = runEndIndex + 1;
                    runEndIndex += runsLength;
                    currentRun = 0;
                }
                currentRun += 1;
            }
            runsLength = GetRunsLength(files, inputFileIndex)[inputFileIndex];
        }
        yield return new MarkRunStep(inputFileIndex, runStartIndex, runEndIndex);
        CloseFiles(files);
    }

    public IEnumerable<IStep> BreakFileIntoRuns(int inputFileIndex)
    {
        var files = OpenFiles(rewriteTempFiles: true);
        var runsLength = GetRunsLength(files, inputFileIndex)[inputFileIndex];

        _lines = 0;

        while (runsLength > 0)
        {
            var currFileIndex = 0;
            var currRunLength = 0;

            var indices = new int[] { -1, -1, -1 };

            var inputFile = files[inputFileIndex].Item1;
            if (currFileIndex == inputFileIndex)
            {
                currFileIndex = (currFileIndex + 1) % _files.Length;
            }
            while (ReadLine(inputFile.BaseStream, true) is string line)
            {
                if (string.IsNullOrEmpty(line)) break;
                _lines++;
                Debug.WriteLine(line);

                if (currRunLength == runsLength)
                {
                    yield return new CommentStep($"Перемещаем подпоследовательность длиной {runsLength} элементов из файла {inputFileIndex} ({indices[inputFileIndex] - (currRunLength - 1)}:{indices[inputFileIndex]})  в файл {currFileIndex} ({indices[currFileIndex] - (currRunLength - 1)}:{indices[currFileIndex]})");
                    yield return new MoveRangeStep
                    {
                        SourceFileIndex = inputFileIndex,
                        TargetFileIndex = currFileIndex,
                        SourceFileItemStartIndex = indices[inputFileIndex] - (currRunLength - 1),
                        TargetFileItemStartIndex = indices[currFileIndex] - (currRunLength - 1),
                        SourceFileItemEndIndex = indices[inputFileIndex],
                        TargetFileItemEndIndex = indices[currFileIndex]
                    };
                    // files[currFileIndex].Item2.Write(line);
                    currFileIndex = (currFileIndex + 1) % _files.Length;
                    if (currFileIndex == inputFileIndex)
                    {
                        currFileIndex = (currFileIndex + 1) % _files.Length;
                    }
                    currRunLength = 0;
                }
                files[currFileIndex].Item2.Write(line);
                files[currFileIndex].Item2.Flush();
                currRunLength += 1;
                indices[currFileIndex] += 1;
                indices[inputFileIndex] += 1;
            }
            yield return new MoveRangeStep
            {
                SourceFileIndex = inputFileIndex,
                TargetFileIndex = currFileIndex,
                SourceFileItemStartIndex = indices[inputFileIndex] - (currRunLength - 1),
                TargetFileItemStartIndex = indices[currFileIndex] - (currRunLength - 1),
                SourceFileItemEndIndex = indices[inputFileIndex],
                TargetFileItemEndIndex = indices[currFileIndex]
            };
            runsLength = GetRunsLength(files, inputFileIndex)[inputFileIndex];
        }
        CloseFiles(files);
    }

    private IEnumerable<IStep> Sort(int inputFileIndex)
    {
        if (_lines == 0) new List<IStep>(BreakFileIntoRuns(inputFileIndex));
        var files = OpenFiles(rewriteTempFiles: false);
        var runs = GetRunsLength(files, inputFileIndex);
        CloseFiles(files);
        while (_lines > runs[inputFileIndex])
        {
            int a = 0;
            yield return new ResetRunColorsStep();
            foreach (var step in FindRuns(inputFileIndex)) yield return step;
            a += 1;
            foreach (var step in BreakFileIntoRuns(inputFileIndex)) yield return step;
            a += 1;
            for (int i = 1; i < _files.Length; i++)
            {
                yield return new ResetRunColorsStep();
                foreach (var step in FindRuns(i)) yield return step;
            }
            a += 1;
            foreach (var step in MergeFilesSingleRun(inputFileIndex, runs[1..].Sum())) yield return step;
            a += 1;
            files = OpenFiles(rewriteTempFiles: false);
            runs = GetRunsLength(files, inputFileIndex);
            CloseFiles(files);
        }
        yield return new RawStep();
    }

    // private IEnumerable<IStep> MergeFiles(int inputFileIndex, int runsLength)
    // {

    // }

    private IEnumerable<IStep> MergeFilesSingleRun(int outputFileIndex, int runsLength)
    {
        var files = OpenFiles(rewriteInputFile: true, rewriteTempFiles: false);
        var outputFile = files[outputFileIndex].Item2;
        var indices = new int[] { 0, 0, 0 };

        while (CanReadAtLeastOneFile(exceptInputFile: true))
        {
            var remainingRuns = new int[] { 0, 0, 0 };

            while (remainingRuns.Sum() <= runsLength)
            {
                // return comparison step
                {
                    var comparisons = new List<(int, int)>();
                    for (int i = 0; i < _files.Length; i++)
                    {
                        if (i == outputFileIndex || remainingRuns[i] >= runsLength) continue;
                        comparisons.Add((i, indices[i]));
                    }
                    yield return new CompareStep(comparisons);
                }

                int minimalFileIndex = GetFileWithMinimalCurrentElement(runsLength, remainingRuns, files, outputFileIndex);
                if (minimalFileIndex == -1) break;
                var currFile = files[minimalFileIndex].Item1;
                var currElement = ReadLine(currFile.BaseStream, true);
                outputFile.Write(currElement);
                outputFile.Flush();

                {
                    yield return new MoveElementStep
                    {
                        SourceFileIndex = minimalFileIndex,
                        TargetFileIndex = outputFileIndex,
                        SourceFileItemIndex = indices[minimalFileIndex],
                        TargetFileItemIndex = indices[outputFileIndex]
                    };
                }
                remainingRuns[minimalFileIndex] += 1;
                indices[minimalFileIndex] += 1;
                indices[outputFileIndex] += 1;
            }
        }
        CloseFiles(files);
    }

    private bool CanReadAtLeastOneFile(bool exceptInputFile = false)
    {
        for (int i = exceptInputFile ? 1 : 0; i < _files.Length; i++)
        {
            if (_files[i].Position != _files[i].Length) return true;
        }
        return false;
    }

    // this method should return the index of the file with minimal current element and skip input file
    private int GetFileWithMinimalCurrentElement(int runsLength, int[] currRunsLength, List<(StreamReader, StreamWriter)> files, int inputFileIndex)
    {
        var currFileIndex = 0 == inputFileIndex ? 1 : 0;
        var result = currFileIndex;
        var minElement = "";
        // find first available file (file is not empty and currRunsLength[i] < runsLength)
        for (int i = 0; i < _files.Length; i++)
        {
            if (i == inputFileIndex || currRunsLength[i] >= runsLength) continue;
            minElement = ReadLine(files[i].Item1.BaseStream, false);
            if (minElement != "")
            {
                result = i;
                break;
            }
        }
        if (minElement == "")
            return -1;
        for (int i = 0; i < _files.Length; i++)
        {
            if (i == inputFileIndex) continue;
            if (currRunsLength[i] < runsLength)
            {
                // there we should get curr element in the file
                var currElement = ReadLine(files[i].Item1.BaseStream, false);
                if (currElement == "") continue;
                if (_comparer.Compare(currElement, minElement) <= 0)
                {
                    minElement = currElement;
                    result = i;
                }
            }
        }
        return result;
    }

    private int[] GetRunsLength(List<(StreamReader, StreamWriter)> files, int inputFileIndex)
    {
        var result = new List<int>() { 0, 0, 0 };
        var prevPositions = new long[_files.Length];
        for (int i = 0; i < _files.Length; i++)
        {
            prevPositions[i] = _files[i].Position;
        }
        var currFileIndex = 0;
        foreach (var file in files)
        {
            var prevLine = "";
            var reader = file.Item1;
            while (ReadLine(reader.BaseStream, true) is string line && !string.IsNullOrEmpty(line.Trim()) && _comparer.Compare(line, prevLine) >= 0)
            {
                prevLine = line;
                result[currFileIndex] += 1;
            }
            currFileIndex++;
        }
        for (int i = 0; i < _files.Length; i++)
        {
            _files[i].Position = prevPositions[i];
        }
        return result.ToArray();
    }

    string ReadLine(Stream sr, bool goToNext)
    {
        if (sr.Position >= sr.Length)
            return string.Empty;
        char readKey;
        StringBuilder strb = new StringBuilder();
        long position = sr.Position;
        do
        {
            readKey = (char)sr.ReadByte();
            strb.Append(readKey);
        }
        while (readKey != '\n' && sr.Position < sr.Length);
        if (!goToNext)
            sr.Position = position;
        return strb.ToString();
    }


    private List<int> GetFilesWithAvailableRuns(int runsLength, int inputFileIndex, List<int> currRunsLength)
    {
        var result = new List<int>();
        for (int i = 0; i < _files.Length; i++)
        {
            if (i == inputFileIndex) continue;
            if (currRunsLength[i] < runsLength) result.Add(i);
        }
        return result;
    }
    private List<(StreamReader, StreamWriter)> OpenFiles(bool rewriteInputFile = false, bool rewriteTempFiles = false)
    {
        var result = new List<(StreamReader, StreamWriter)>();
        _files[0].Close();
        _files[0] = System.IO.File.Open(_files[0].Name, rewriteInputFile ? FileMode.Create : FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        result.Add((new StreamReader(_files[0]), new StreamWriter(_files[0])));
        result.Last().Item2.AutoFlush = true;
        for (int i = 1; i < _files.Length; i++)
        {
            int index = i;
            var fileName = _files[index].Name;
            _files[index].Close();
            _files[index] = System.IO.File.Open(fileName, rewriteTempFiles ? FileMode.Create : FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            result.Add((new StreamReader(_files[index]), new StreamWriter(_files[index])));
            result.Last().Item2.AutoFlush = true;
        }
        return result;
    }

    private void CloseFiles(List<(StreamReader, StreamWriter)> files)
    {
        foreach (var file in files)
        {
            file.Item1.Close();
        }
        foreach (var file in _files)
        {
            file.Close();
        }
    }

    public IEnumerable<IStep> Run()
    {
        _files = [
            new FileStream(_inputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read),
            new FileStream("2.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read),
            new FileStream("3.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read)
        ];
        foreach (var step in Sort(0))
        {
            yield return step;
        }

    }
}