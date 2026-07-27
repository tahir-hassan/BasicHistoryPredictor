using System.Management.Automation;
using System.Management.Automation.Subsystem;

namespace BasicHistoryPredictor;


[Cmdlet(VerbsLifecycle.Register, nameof(BasicHistoryPredictor))]
public class RegisterBasicHistoryPredictorCmdlet : PSCmdlet
{
    protected override void ProcessRecord()
    {
        var intrinsics = (this.GetVariableValue("ExecutionContext") as EngineIntrinsics)!;
        var predictor = new ThePredictor(intrinsics);
        SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, predictor);
        WriteVerbose($"Registered {typeof(ThePredictor).FullName}");
    }
}

[Cmdlet(VerbsLifecycle.Unregister, nameof(BasicHistoryPredictor))]
public class UnRegisterBasicHistoryPredictorCmdlet : PSCmdlet
{
    protected override void ProcessRecord()
    {
        SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, ThePredictor.SubsystemIdentifier);
        WriteVerbose($"Unregistered {typeof(ThePredictor).FullName}");
    }
}

[Cmdlet(VerbsCommon.Remove, "SelectedHistoryItem")]
public class RemoveSelectedHistoryItemCmdlet : PSCmdlet
{
    private static readonly string restorePositionInList = @"
[Microsoft.PowerShell.PSConsoleReadLine]::RevertLine();
[Microsoft.PowerShell.PSConsoleReadLine]::Insert(' ');
[Microsoft.PowerShell.PSConsoleReadLine]::BackwardDeleteChar();

if ($ind -ne -1) {
	for ($i = 0; $i -le $ind; $i++) {
		[Microsoft.PowerShell.PSConsoleReadLine]::NextHistory();
	}
}
0;";

    protected override void ProcessRecord()
    {
        using var psConsole = new PSConsole(RunspaceMode.CurrentRunspace);
        var currentLine = psConsole.GetBufferLine();
        var index = ThePredictor.Instance!.RemoveItem(currentLine);
        psConsole.Invoke<int>(restorePositionInList.Replace("$ind", index.ToString()));
    }
}