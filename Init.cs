using System.Management.Automation;

namespace BasicHistoryPredictor;

/// <summary>
/// Register the predictor on module loading and unregister it on module un-loading.
/// </summary>
public class Init : IModuleAssemblyInitializer, IModuleAssemblyCleanup
{
    /// <summary>
    /// Gets called when assembly is loaded.
    /// 
    /// Use Unregister-BasicHistoryPredictor <see cref="RegisterBasicHistoryPredictorCmdlet"/>
    /// to load the predictor
    /// </summary>
    public void OnImport()
    {
        // do nothing
    }

    /// <summary>
    /// Gets called when the binary module is unloaded.
    /// 
    /// Use Unregister-BasicHistoryPredictor <see cref="UnRegisterBasicHistoryPredictorCmdlet"/>
    /// to unload the predictor
    /// </summary>
    public void OnRemove(PSModuleInfo psModuleInfo)
    {
        // do nothing
    }
}
