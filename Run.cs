namespace lab4_extra;

public class Run
{
    public int RunIndex { get; set; }
    public int RunStartIndex { get; set; }
    public int RunEndIndex { get; set; } 

    public Run(int runIndex, int runStartIndex, int runEndIndex)
    {
        RunIndex = runIndex;
        RunStartIndex = runStartIndex;
        RunEndIndex = runEndIndex;
    }
}