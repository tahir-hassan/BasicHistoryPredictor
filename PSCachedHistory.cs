namespace BasicHistoryPredictor;

public class PSCachedHistory(PSConsole psConsoleReadLine)
{
    private readonly PSConsole psConsoleReadLine = psConsoleReadLine;

    private List<string>? _cachedHistory = null;

    private void EnsureInitialized()
    {
        if (_cachedHistory == null)
        {
            _cachedHistory = psConsoleReadLine.GetHistoryCommandLines().ToList();
        }
    }

    public IEnumerable<string> GetCachedHistory()
    {
        lock (this)
        {
            EnsureInitialized();

            return _cachedHistory!.AsReadOnly();
        }
    }

    public void AddToCachedHistory(string line)
    {

        if (!string.IsNullOrWhiteSpace(line))
        {
            lock (this)
            {
                EnsureInitialized();

                _cachedHistory!.Add(line);
            }
        }
    }

    public void RemoveFromCachedHistory(string line)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            lock (this)
            {
                EnsureInitialized();

                _cachedHistory!.RemoveAll(x => x == line);
            }
        }
    }
}