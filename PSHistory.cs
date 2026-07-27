namespace BasicHistoryPredictor;

public static class PSHistory
{
    public static string SuggestedLineToStoredLine(string suggestedLine)
    {
        if (suggestedLine.Contains('\n')) 
        {
            return suggestedLine.Replace("\n", "`\n");
        } 
        else
        {
            return suggestedLine;

        }
    }
    public static string[] GetHistoryLines(string historySavePath)
    {
        var text = File.ReadAllText(historySavePath);
        var lines = text.Split(["\r\n"], StringSplitOptions.None);
        return lines;
    }

    public static void SetHistoryLines(string historySavePath, string[] lines)
    {
        var text = string.Join("\r\n", lines);
        File.WriteAllText(historySavePath, text);
    }
}
