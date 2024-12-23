using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class NWayMergeSort<T>
{
    private readonly IComparer<T> _comparer;
    private readonly int _numberOfWays;

    public NWayMergeSort(int numberOfWays, IComparer<T> comparer)
    {
        _numberOfWays = numberOfWays;
        _comparer = comparer;
    }

    public void Sort(string inputFile, string outputFile)
    {
        var tempFiles = InitializeTempFiles();
        Divide(inputFile, tempFiles);
        Merge(tempFiles, outputFile);
        foreach (var file in tempFiles) File.Delete(file);
    }

    private List<string> InitializeTempFiles()
    {
        var tempFiles = new List<string>();
        for (int i = 0; i < _numberOfWays; i++)
        {
            tempFiles.Add(Path.GetTempFileName());
        }
        return tempFiles;
    }

    private void Divide(string inputFile, List<string> tempFiles)
    {
        using (var reader = new StreamReader(inputFile))
        {
            int index = 0;
            while (!reader.EndOfStream)
            {
                using (var writer = new StreamWriter(tempFiles[index], append: true))
                {
                    writer.WriteLine(reader.ReadLine());
                }
                index = (index + 1) % _numberOfWays;
            }
        }
    }

    private void Merge(List<string> tempFiles, string outputFile)
    {
        while (true)
        {
            var readers = tempFiles.Select(file => new StreamReader(file)).ToList();
            var currentLines = new Dictionary<StreamReader, T>();
            var tempWriters = tempFiles.Select(file => new StreamWriter(file + ".tmp")).ToList();
            int activeFiles = 0;

            for (int i = 0; i < readers.Count; i++)
            {
                if (!readers[i].EndOfStream)
                {
                    currentLines[readers[i]] = (T)Convert.ChangeType(readers[i].ReadLine(), typeof(T));
                    activeFiles++;
                }
            }

            if (activeFiles == 0) break;

            while (currentLines.Count > 0)
            {
                var minEntry = currentLines.OrderBy(kv => kv.Value, _comparer).First();
                tempWriters[tempFiles.IndexOf(minEntry.Key.BaseStream is FileStream fs ? fs.Name : "")].WriteLine(minEntry.Value);
                if (!minEntry.Key.EndOfStream)
                {
                    currentLines[minEntry.Key] = (T)Convert.ChangeType(minEntry.Key.ReadLine(), typeof(T));
                }
                else
                {
                    minEntry.Key.Dispose();
                    currentLines.Remove(minEntry.Key);
                }
            }

            foreach (var reader in readers) reader.Dispose();
            foreach (var writer in tempWriters) writer.Dispose();

            for (int i = 0; i < tempFiles.Count; i++)
            {
                File.Delete(tempFiles[i]);
                File.Move(tempFiles[i] + ".tmp", tempFiles[i]);
            }
        }

        File.Move(tempFiles[0], outputFile, true);
    }
}
