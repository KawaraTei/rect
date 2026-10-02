using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace LectImageRenamer.Models;

public static class RenameFormat
{
    private static readonly Regex Tokens = new(@"\\n+|\{\}");

    public static string CreateFileName(string format, string originalFileName, long number, int defaultPaddingWidth)
    {
        string pattern = format.Trim();
        string stem = Tokens.IsMatch(pattern)
            ? Tokens.Replace(pattern, match => match.Value == "{}"
                ? Path.GetFileNameWithoutExtension(originalFileName)
                : number.ToString(CultureInfo.InvariantCulture).PadLeft(match.Length - 1, '0'))
            : $"{pattern}_{number.ToString(CultureInfo.InvariantCulture).PadLeft(defaultPaddingWidth, '0')}";

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            stem = stem.Replace(invalidChar, '_');
        }

        return stem + Path.GetExtension(originalFileName);
    }
}
