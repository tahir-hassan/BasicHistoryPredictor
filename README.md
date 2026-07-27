# How to use

After compiling it, you will end up with a `.dll` in the path:

```
BasicHistoryPredictor\bin\Debug\net8.0\BasicHistoryPredictor.dll
```

After copying the `net8.0` build folder somewhere, you can then do this in PowerShell:

```
Import-Module "path/to/BasicHistoryPredictor.dll"
Register-BasicHistoryPredictor
Set-PSReadLineOption -PredictionSource Plugin
Set-PSReadLineOption -PredictionViewStyle ListView
``` 

# Removing History Items

You can bind `Ctrl+d` to `Remove-SelectedHistoryItem` so that when you are in the suggestions list, pressing Control+d will remove the selected item from the history:

```powershell
Set-PSReadLineKeyHandler -Chord Ctrl+d -ScriptBlock {
	Remove-SelectedHistoryItem 
}
```

# Dealing with Visual Remnants

When the list is displayed, if it is too large or contains multiline items, remnants of the list remain.  I have re-bound `Escape` and `Backspace` to clear it all up:

```powershell
using namespace Microsoft.PowerShell;

# ...

$vtCodes = [pscustomobject]@{
	clearEos = "`e[J"
	clearEol = "`e[K"
};

function _getBufferLine {
	$line = $null
	$cursor = $null
	[PSConsoleReadLine]::GetBufferState(
		[ref]$line,
		[ref]$cursor
	);
	$line;
}

Set-PSReadlineKeyHandler -Key Escape -ScriptBlock {
	[PSConsoleReadLine]::RevertLine();
	[Console]::Write($vtCodes.clearEos);
}

Set-PSReadlineKeyHandler -Key Backspace -ScriptBlock {
	[PSConsoleReadLine]::BackwardDeleteChar();
	if ((_getBufferLine) -eq '') {
		[Console]::Write($vtCodes.clearEos);
	}
}

Set-PSReadLineKeyHandler -Key 'DownArrow' -ScriptBlock { 
	[PSConsoleReadLine]::NextHistory(); 
	[Console]::Write($vtCodes.clearEol) 
}

Set-PSReadLineKeyHandler -Key 'UpArrow' -ScriptBlock { 
	[PSConsoleReadLine]::PreviousHistory(); 
	[Console]::Write($vtCodes.clearEol) 
}
```

# Sources

I followed [How to create a command-line predictor](https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/create-cmdline-predictor?view=powershell-7.5)
to create this predictor.

Some of the code was taken from [CompletionPredictor](https://github.com/PowerShell/CompletionPredictor) such as the use of runspaces to execute code.

# .NET version

When running `dotnet`, it created a project with a .NET version of 8.  

I have used the most up to date NuGet package for Microsoft.PowerShell.SDK for 7.4.x.

# TODO

- Put this on PSGallery.
