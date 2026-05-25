// Carries the current merge objective progress to UI listeners.
public readonly struct MergeProgressChangedEvent
{
    public int CompletedMergeCount { get; }
    public int TotalMergeCount { get; }

    // Creates a progress event with completed and required merge counts.
    public MergeProgressChangedEvent(int completedMergeCount, int totalMergeCount)
    {
        CompletedMergeCount = completedMergeCount;
        TotalMergeCount = totalMergeCount;
    }
}
