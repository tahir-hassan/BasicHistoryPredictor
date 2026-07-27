using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Subsystem.Prediction;
using System.Text.RegularExpressions;

namespace BasicHistoryPredictor;

public partial class ThePredictor : ICommandPredictor, IDisposable
{
    public static readonly Guid SubsystemIdentifier = new("dea9d976-6bfc-4726-857d-3bf2339dc1ba");

    private readonly Runspace _runspace;
    private readonly PSConsole psConsoleReadLine;
    private readonly PSCachedHistory cachedHistory;
    private string[]? lastShownHistoryItems;
    public static List<string> ErrorMessages = [];
    public static ThePredictor? Instance { get; private set;  }


    internal ThePredictor(EngineIntrinsics intrinsics)
    {
        Intrinsics = intrinsics;

        var sessionState = InitialSessionState.CreateDefault();
        
        _runspace = RunspaceFactory.CreateRunspace(sessionState);
        _runspace.Name = nameof(ThePredictor);
        _runspace.Open();

        psConsoleReadLine = new PSConsole(_runspace);
        cachedHistory = new PSCachedHistory(psConsoleReadLine);
        Instance = this;
    }

    /// <summary>
    /// Gets the unique identifier for a subsystem implementation.
    /// </summary>
    public Guid Id => SubsystemIdentifier;
     /// <summary> 
     /// Gets the name of a subsystem implementation.  
     /// </summary> 
    public string Name => "History"; 
    /// <summary>
    /// Gets the description of a subsystem implementation.
    /// </summary>
    public string Description => "A more forgiving history predictor";

    public EngineIntrinsics Intrinsics { get; }

    public IEnumerable<string> GetCachedHistory()
    {
        return cachedHistory.GetCachedHistory();
    }
    private PredictiveSuggestion CreatePredictiveSuggestion(string item)
    {
        return new PredictiveSuggestion(item);
    }

    private void SendPromptToTop()
    {
        var cursorTop = Console.CursorTop;
        var cursorLeft = Console.CursorLeft;
        if (cursorTop > 1)
        {
            Console.SetCursorPosition(Console.BufferWidth - 1, Console.BufferHeight - 1);
            Console.Write(new string('\n', cursorTop));
            Console.SetCursorPosition(cursorLeft, 0);
        }
    }

    private void ClearAreaBelowCursor()
    {
        var windowSize = Intrinsics.Host.UI.RawUI.WindowSize;
        var cursorPos = Intrinsics.Host.UI.RawUI.CursorPosition;
        if (cursorPos.Y < windowSize.Height)
        {
            Intrinsics.Host.UI.RawUI.SetBufferContents(new Rectangle(0, cursorPos.Y + 1, windowSize.Width, windowSize.Height), new BufferCell());
        }
    }
   
    /// <summary>
    /// Get the predictive suggestions. It indicates the start of a suggestion rendering session.
    /// </summary>
    /// <param name="client">Represents the client that initiates the call.</param>
    /// <param name="context">The <see cref="PredictionContext"/> object to be used for prediction.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the prediction.</param>
    /// <returns>An instance of <see cref="SuggestionPackage"/>.</returns>
    public SuggestionPackage GetSuggestion(PredictionClient client, PredictionContext context, CancellationToken cancellationToken)
    {
        try
        {
            ClearAreaBelowCursor();

            lastShownHistoryItems = null;
            string input = context.InputAst.Extent.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                return new SuggestionPackage([]);
            }
            else
            {
                var historyItems = GetHistoryItems(input);
                lastShownHistoryItems = historyItems;

                if (historyItems.Take(10).Any(item => item.Contains('\n')))
                {
                    SendPromptToTop();
                }
                
                return new SuggestionPackage(historyItems.Select(CreatePredictiveSuggestion).ToList());
            }
        }
        catch (Exception ex)
        {
            ErrorMessages.Add($"{ex.GetType().Name} - {ex.Message}");
            return new SuggestionPackage([]);
        }
    }

    #region "interface methods for processing feedback"

    /// <summary>
    /// Gets a value indicating whether the predictor accepts a specific kind of feedback.
    /// </summary>
    /// <param name="client">Represents the client that initiates the call.</param>
    /// <param name="feedback">A specific type of feedback.</param>
    /// <returns>True or false, to indicate whether the specific feedback is accepted.</returns>
    public bool CanAcceptFeedback(PredictionClient client, PredictorFeedbackKind feedback) => feedback == PredictorFeedbackKind.CommandLineExecuted;

    /// <summary>
    /// One or more suggestions provided by the predictor were displayed to the user.
    /// </summary>
    /// <param name="client">Represents the client that initiates the call.</param>
    /// <param name="session">The mini-session where the displayed suggestions came from.</param>
    /// <param name="countOrIndex">
    /// When the value is greater than 0, it's the number of displayed suggestions from the list
    /// returned in <paramref name="session"/>, starting from the index 0. When the value is
    /// less than or equal to 0, it means a single suggestion from the list got displayed, and
    /// the index is the absolute value.
    /// </param>
    public void OnSuggestionDisplayed(PredictionClient client, uint session, int countOrIndex) { }

    /// <summary>
    /// The suggestion provided by the predictor was accepted.
    /// </summary>
    /// <param name="client">Represents the client that initiates the call.</param>
    /// <param name="session">Represents the mini-session where the accepted suggestion came from.</param>
    /// <param name="acceptedSuggestion">The accepted suggestion text.</param>
    public void OnSuggestionAccepted(PredictionClient client, uint session, string acceptedSuggestion) { }

    /// <summary>
    /// A command line was accepted to execute.
    /// The predictor can start processing early as needed with the latest history.
    /// </summary>
    /// <param name="client">Represents the client that initiates the call.</param>
    /// <param name="history">History command lines provided as references for prediction.</param>
    public void OnCommandLineAccepted(PredictionClient client, IReadOnlyList<string> history) { }

    /// <summary>
    /// A command line was done execution.
    /// </summary>
    /// <param name="client">Represents the client that initiates the call.</param>
    /// <param name="commandLine">The last accepted command line.</param>
    /// <param name="success">Shows whether the execution was successful.</param>
    public void OnCommandLineExecuted(PredictionClient client, string commandLine, bool success) 
    {
        if (success)
        {
            cachedHistory.AddToCachedHistory(commandLine);
        }
    }

    public void Dispose()
    {
        _runspace.Dispose();
    }

    #endregion;

    public string[] GetHistoryItems(string input)
    {
        input = input.Trim();

        var history = cachedHistory.GetCachedHistory();

        if (!history.Any())
        {
            return [];
        }
        else
        {
            var modifiedInput = input.Replace("(", "( ").Replace(")", " )").Replace("[", "[ ").Replace("]", " ]");
            var inputRegexString = Regex.Escape(modifiedInput).Replace(@"\ ", ".*");
            var regexInput = new Regex(inputRegexString, RegexOptions.IgnoreCase);
            var historyMatches = history.Reverse().Distinct().Select(x => new
            {
                item = x,
                match = regexInput.Match(x)
            }).Where(x => x.match.Success).ToArray();
            
            var startsWith = historyMatches.ToLookup(x => x.match.Index == 0);

            return startsWith[true].Concat(startsWith[false]).Select(x => x.item).ToArray();
        }

    }

    /// <summary>
    /// removes an item from the history
    /// </summary>
    /// <param name="item">item to remove from history</param>
    /// <returns>index of the removed item</returns>
    public int RemoveItem(string item)
    {
        int index = (lastShownHistoryItems != null) ? Array.IndexOf(lastShownHistoryItems, item) : -1;
        
        cachedHistory.RemoveFromCachedHistory(item);

        var historySavePath = psConsoleReadLine.GetHistorySavePath();
        var lines = PSHistory.GetHistoryLines(historySavePath);
        var newLines = lines.Except([PSHistory.SuggestedLineToStoredLine(item)]).ToArray();
        PSHistory.SetHistoryLines(historySavePath, newLines);

        return index;
    }
}
