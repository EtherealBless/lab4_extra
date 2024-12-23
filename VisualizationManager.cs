using System.Windows.Media;
using lab4_extra.Steps;
using lab4_extra.ViewModels;
using Microsoft.Win32;

namespace lab4_extra;

public class VisualizationManager
{
    private IAlgorithm _algorithm;
    private FilesVM _filesVM;
    private List<FilesVM> _states = new List<FilesVM>();
    private List<IStep> _steps = new List<IStep>();
    private int _stepIndex = 0;
    private int _stateIndex = 0;
    private string[] _data;
    public bool CanStepForward => _stepIndex < _steps.Count;
    public bool CanStepBack => _stateIndex > 0;
    public VisualizationManager(IAlgorithm algorithm, FilesVM filesVM)
    {
        _algorithm = algorithm;
        _filesVM = filesVM;
    }

    public void WarmUp()
    {
        _steps = new List<IStep>(_algorithm.Run());
        _states = new List<FilesVM>();
        for (int i = 0; i < _steps.Count; i++)
        {
            _states.AddRange(DoStep(_steps[i]));
        }
    }

        /// <summary>
        /// Loads data from a file to the visualization.
        /// </summary>
        /// <param name="inputPath">Path to the file with the data.</param>
        /// <param name="columnIndex">Index of the column in the file, which should be used as data.</param>
        /// <param name="baseFile">Index of the base file in the visualization. Data from the file will be loaded to this file.</param>
        /// <param name="separator">Separator used in the file to separate columns. Default is ','.</param>
    public void LoadData(string inputPath, int columnIndex, int baseFile = 0, char separator = ',')
    {
        _data = System.IO.File.ReadAllLines(inputPath).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        _filesVM.Files[baseFile].Items = new ItemVM[_data.Length];
        for (int i = 0; i < _data.Length; i++)
        {
            _filesVM.Files[baseFile].Items[i] = new ItemVM(_data[i].Split(separator)[columnIndex]);
        }
        for (int i = 0; i < _filesVM.Files.Length; i++)
        {
            if (i == baseFile) continue;
            _filesVM.Files[i].Items = new ItemVM[_data.Length];
            for (int j = 0; j < _data.Length; j++)
            {
                _filesVM.Files[i].Items[j] = new ItemVM("");
            }
        }
    }

    public FilesVM StepForward()
    {
        if (_states.Count > _stateIndex)
        {
            _filesVM = _states[_stateIndex++];
            return _filesVM;
        }
        return _filesVM;
    }
    public FilesVM StepBack()
    {
        if (_stateIndex > 0)
        {
            _filesVM = _states[--_stateIndex];
            return _filesVM;
        }
        return _filesVM;
    }
    private string GetItemValue(int fileIndex, int itemIndex) => _filesVM.Files[fileIndex].Items[itemIndex].Value;
    private List<RawStep>? UnrollStep(IStep step)
    {
        if (step is MoveElementStep moveStep)
        {
            // 1. mark source cell as red
            // 2. mark destination cell as yellow
            // 3. move element
            // 4. mark destination cell as red
            // 5. mark source cell as default color
            // 6. mark destination cell as default color
            var result = new List<RawStep>
            {
                new RawColorStep(moveStep.SourceFileIndex, moveStep.SourceFileItemIndex, Colors.Red),
                new RawColorStep(moveStep.TargetFileIndex, moveStep.TargetFileItemIndex, Colors.Yellow),
                new FlushStep(),
                new RawMoveStep(moveStep.SourceFileIndex, moveStep.SourceFileItemIndex, moveStep.TargetFileIndex, moveStep.TargetFileItemIndex),
                new RawColorStep(moveStep.TargetFileIndex, moveStep.TargetFileItemIndex, Colors.Red),
                new FlushStep(),
                new RawColorStep(moveStep.SourceFileIndex, moveStep.SourceFileItemIndex, Colors.White),
                new RawColorStep(moveStep.TargetFileIndex, moveStep.TargetFileItemIndex, Colors.White),
                new FlushStep()
            };
            return result;
        }
        else if (step is MoveRangeStep moveRangeStep)
        {
            var result = new List<RawStep>();
            for (int i = moveRangeStep.SourceFileItemStartIndex; i <= moveRangeStep.SourceFileItemEndIndex; i++)
            {
                result.AddRange(UnrollStep(new MoveElementStep()
                {
                    SourceFileIndex = moveRangeStep.SourceFileIndex,
                    SourceFileItemIndex = i,
                    TargetFileIndex = moveRangeStep.TargetFileIndex,
                    TargetFileItemIndex = moveRangeStep.TargetFileItemStartIndex + (i - moveRangeStep.SourceFileItemStartIndex)
                })!);
            }
            return result;
        }
        else if (step is CompareStep compareStep)
        {
            var result = new List<RawStep>();
            for (int i = 0; i < compareStep.Comparisons.Count; i++)
            {
                result.Add(new RawColorStep(compareStep.Comparisons[i].Item1, compareStep.Comparisons[i].Item2, Colors.Red));
            }
            result.Add(new FlushStep());
            return result;
        }
        return null;
    }

    /// <summary>
    /// Applies the given step to the visualization model.
    /// If the step is a compound step, it is broken down into raw steps.
    /// If the step is a raw step or a compound step that contains a flush step,
    /// the current state of the visualization model is yielded as it is after application of the step.
    /// </summary>
    /// <param name="step">The step to apply.</param>
    /// <returns>An enumeration of visualization models, each representing the state
    /// after application of the respective step.</returns>
    private IEnumerable<FilesVM> DoStep(IStep step)
    {
        if (UnrollStep(step) is List<RawStep> rawSteps)
        {
            foreach (var rawStep in rawSteps)
            {
                if (rawStep is FlushStep flushStep)
                {
                    yield return _filesVM.Clone();
                    continue;
                }
                DoSingleStep(rawStep);
            }
        }
        else if (step is not RawStep)
        {
            DoSingleStep(step);
            yield return _filesVM.Clone();
        }
        // yield return _filesVM.Clone();
    }

    private void DoSingleStep(IStep step)
    {
        if (step is RawColorStep colorStep)
        {
            if (_filesVM.Files[colorStep.FileIndex].Items[colorStep.FileItemIndex].Value != "")
                _filesVM.Files[colorStep.FileIndex].Items[colorStep.FileItemIndex].Color = colorStep.Color;
        }
        else if (step is RawMoveStep moveStep)
        {
            _filesVM.Files[moveStep.TargetFileIndex].Items[moveStep.TargetFileItemIndex] = _filesVM.Files[moveStep.SourceFileIndex].Items[moveStep.SourceFileItemIndex];
            _filesVM.Files[moveStep.SourceFileIndex].Items[moveStep.SourceFileItemIndex] = new ItemVM("");
        }
        else if (step is MarkRunStep runStep)
        {
            var runColor = runStep.Color;
            for (int i = runStep.StartIndex; i <= Math.Min(_filesVM.Files[runStep.FileIndex].Items.Length - 1, runStep.EndIndex); i++)
            {
                if (_filesVM.Files[runStep.FileIndex].Items[i].Value != "")
                    _filesVM.Files[runStep.FileIndex].Items[i].Color = runColor;
            }
        }
        else if (step is CommentStep commentStep)
        {
            _filesVM.StepsDescription.Add(commentStep.Comment);
        }
        else if (step is ResetRunColorsStep)
        {
            MarkRunStep.ResetColorIndex();
        }
    }
}
