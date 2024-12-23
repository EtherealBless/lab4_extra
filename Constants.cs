using System.Windows.Media;

namespace lab4_extra.Constants;

public static class ColorsConstants
{
    public static readonly Color DefaultCell = Colors.White;
    public static readonly Color SelectedCell = Colors.Green;
    public static readonly Color MergedSourceCell = Colors.LightBlue;
    public static readonly Color MergedTargetCell = Colors.LightGreen;
    public static readonly List<Color> RunColorList = [
        Colors.LightYellow,
        Colors.LightSalmon,
        Colors.LightSeaGreen,
        Colors.LightSkyBlue,
        Colors.LightPink,
        Colors.LightSlateGray
    ];
}