using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Markdig.Helpers;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace BasicHistoryPredictor;

public class HistoryItem
{
	public string CommandLine { get; set; }
	public DateTime? StartTime { get; set; }

    public HistoryItem(string commandLine, DateTime? startTime)
    {
		this.CommandLine = commandLine;
		this.StartTime = StartTime;
    }
}
public partial class ThePredictor
{
	char[] rns = new char[] { '\r', '\n' };

	private ConcurrentDictionary<string, string> EllipsisDict = new ConcurrentDictionary<string, string>();
	private List<HistoryItem>? _historyItems = null;
	private List<string> _removedItems = new();
	private string? _historySavePath = null;
	private string Ellipsis(string str)
	{
		if (EllipsisDict.ContainsKey(str))
			return EllipsisDict[str];
		else
		{
            var lineBreakIndex = str.IndexOfAny(rns);
			var result = "";
            if (lineBreakIndex == -1)
			{
				result = str;
			}
            else
			{
                result = str.Substring(0, lineBreakIndex) + "...";
			}
			EllipsisDict.TryAdd(str, result);
			return result;
		}
	}

	private List<T> InvokePowerShell<T>(string script)
	{
        using var ps = PowerShell.Create();
        ps.Runspace = _runspace;

        var all =
            ps.AddScript(script)
            .InvokeAndCleanup<T>().ToList();

		return all;
    }

	private List<HistoryItem> GetHistoryItems()
	{
		var all = InvokePowerShell<HistoryItem>("[Microsoft.PowerShell.PSConsoleReadLine]::GetHistoryItems() | ForEach-Object { [BasicHistoryPredictor.HistoryItem]::new($_.CommandLine, $_.StartTime) }");
		all.Reverse();
		return all;
	}

	private string GetHistorySavePath()
	{
		if (_historySavePath == null)
		{
			_historySavePath = InvokePowerShell<string>("[Microsoft.PowerShell.PSConsoleReadLine]::GetOptions().HistorySavePath").First();
        }
		return _historySavePath!;
    }
	public string[] GetHistoryItems(string input)
	{
		input = input.Trim();
		if (_historyItems == null)
			_historyItems = GetHistoryItems();

		if (!_historyItems.Any())
			return [];

		var regexInput = new Regex(Regex.Escape(input).Replace(@"\ ", ".*"), RegexOptions.IgnoreCase);

		return _historyItems.Select(x => x.CommandLine).Except(_removedItems).Where(x => regexInput.IsMatch(x)).Select(Ellipsis).Distinct().ToArray();
	}

	public void RemoveItem(string item)
	{
		_removedItems.Add(item);

		var historySavePath = GetHistorySavePath();
		var lines = PSHistory.GetHistoryLines(historySavePath);
		var newLines = lines.Except([item]).ToArray();
		PSHistory.SetHistoryLines(historySavePath, newLines);
	}
}
