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
	private List<HistoryItem> GetHistoryItems()
	{
		using var ps = PowerShell.Create();
		ps.Runspace = _runspace;

		var all =
			ps.AddScript("[Microsoft.PowerShell.PSConsoleReadLine]::GetHistoryItems() | ForEach-Object { [BasicHistoryPredictor.HistoryItem]::new($_.CommandLine, $_.StartTIme) }")
			.InvokeAndCleanup<HistoryItem>().ToList();

		if (!all.Any())
			return [];

		all.Reverse();

		return all;
	}
	public string[] GetHistoryItems(string input)
	{
		input = input.Trim();
		if (_historyItems == null)
			_historyItems = GetHistoryItems();

		if (!_historyItems.Any())
			return [];

		var regexInput = new Regex(Regex.Escape(input).Replace(@"\ ", ".*"), RegexOptions.IgnoreCase);

		return _historyItems.Select(x => x.CommandLine).Where(x => regexInput.IsMatch(x)).Select(Ellipsis).Distinct().ToArray();
	}

}
