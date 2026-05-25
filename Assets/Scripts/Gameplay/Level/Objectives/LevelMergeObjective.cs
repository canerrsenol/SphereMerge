using UnityEngine;
using VContainer;

// Completes a level after all required sphere merges are performed.
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
    // Receives the sphere grid and game state service.
    public void Construct(SpheresManager spheresManager, IGameStateService gameStateService)
    {
        this.spheresManager = spheresManager;
        this.gameStateService = gameStateService;
    }

    // Starts listening for completed sphere merges.
    private void OnEnable()
    {
        EventBus.Subscribe<SpheresMergedEvent>(HandleSpheresMerged);
    }

    // Calculates how many merges this level requires.
    private void Start()
    {
        InitializeObjective();
    }

    // Stops listening for completed sphere merges.
    private void OnDisable()
    {
        EventBus.Unsubscribe<SpheresMergedEvent>(HandleSpheresMerged);
    }

    // Creates the merge target from the number of spheres in the level.
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

    // Updates progress and completes the level when the target is reached.
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

    // Publishes current progress for HUD elements.
    private void PublishProgress()
    {
        EventBus.Publish(new MergeProgressChangedEvent(completedMergeCount, totalMergeCount));
    }
}
