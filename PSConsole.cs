using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace BasicHistoryPredictor;

public class PSConsole: IDisposable
{
    private readonly PowerShell ps;
    public PSConsole(Runspace _runspace)
    {
        ps = PowerShell.Create(_runspace);
    }
    public PSConsole(RunspaceMode runspaceMode)
    {
        ps = PowerShell.Create(runspaceMode);
    }
    
    public Collection<T> Invoke<T>(string script)
    {
        lock (this)
        {
            var results = ps.AddScript(script).Invoke<T>();
            ps.Commands.Clear();
            return results;
        }
    }

    public string GetHistorySavePath()
    {
        return Invoke<string>("[Microsoft.PowerShell.PSConsoleReadLine]::GetOptions().HistorySavePath").First();
    }

    public Collection<string> GetHistoryCommandLines()
    {
        return Invoke<string>("[Microsoft.PowerShell.PSConsoleReadLine]::GetHistoryItems() | % CommandLine");
    }

    public string GetBufferLine()
    {
        return Invoke<string>(@"
                $line = $null;
                $cursor = $null;
                [Microsoft.PowerShell.PSConsoleReadLine]::GetBufferState([ref]$line,[ref]$cursor);
                $line;
                ").First();
    }

    public void Dispose()
    {
        ps.Dispose();
    }
}
