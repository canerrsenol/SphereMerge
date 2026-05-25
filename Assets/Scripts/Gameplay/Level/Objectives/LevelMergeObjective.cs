using UnityEngine;
using VContainer;

[DisallowMultipleComponent]
public sealed class LevelMergeObjective : MonoBehaviour
{
    private const int SpheresPerMerge = 3;

    private SpheresManager spheresManager;
    private IGameStateService gameStateService;
    private int totalMergeCount;
    private int completedMergeCount;
    private bool initialized;
    private bool levelCompleted;

    [Inject]
    public void Construct(SpheresManager spheresManager, IGameStateService gameStateService)
    {
        this.spheresManager = spheresManager;
        this.gameStateService = gameStateService;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<SpheresMergedEvent>(HandleSpheresMerged);
    }

    private void Start()
    {
        InitializeObjective();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SpheresMergedEvent>(HandleSpheresMerged);
    }

    private void InitializeObjective()
    {
        if (spheresManager == null)
        {
            Debug.LogError("SpheresManager was not injected into LevelMergeObjective.", this);
            return;
        }

        int sphereCount = spheresManager.GetSphereCount();
        if (sphereCount <= 0)
        {
            Debug.LogWarning("LevelMergeObjective found no spheres to merge.", this);
            initialized = true;
            PublishProgress();
            return;
        }

        if (sphereCount % SpheresPerMerge != 0)
        {
            Debug.LogError(
                $"Level contains {sphereCount} spheres, which cannot be fully cleared in groups of {SpheresPerMerge}.",
                this);
            initialized = true;
            PublishProgress();
            return;
        }

        totalMergeCount = sphereCount / SpheresPerMerge;
        initialized = true;
        PublishProgress();
    }

    private void HandleSpheresMerged(SpheresMergedEvent mergeEvent)
    {
        if (!initialized
            || levelCompleted
            || totalMergeCount <= 0
            || mergeEvent.MergedSpheres == null
            || mergeEvent.MergedSpheres.Count == 0)
        {
            return;
        }

        completedMergeCount = Mathf.Min(completedMergeCount + 1, totalMergeCount);
        PublishProgress();

        if (completedMergeCount < totalMergeCount)
        {
            return;
        }

        levelCompleted = true;
        if (gameStateService == null)
        {
            Debug.LogError("IGameStateService was not injected into LevelMergeObjective.", this);
            return;
        }

        gameStateService.ChangeState(GameState.LevelCompleted);
    }

    private void PublishProgress()
    {
        EventBus.Publish(new MergeProgressChangedEvent(completedMergeCount, totalMergeCount));
    }
}
