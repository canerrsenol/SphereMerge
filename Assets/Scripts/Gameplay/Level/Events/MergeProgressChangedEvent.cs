public readonly struct MergeProgressChangedEvent
{
    public int CompletedMergeCount { get; }
    public int TotalMergeCount { get; }

    public MergeProgressChangedEvent(int completedMergeCount, int totalMergeCount)
    {
        CompletedMergeCount = completedMergeCount;
        TotalMergeCount = totalMergeCount;
    }
}
