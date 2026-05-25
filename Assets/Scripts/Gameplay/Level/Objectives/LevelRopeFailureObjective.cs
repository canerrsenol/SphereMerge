using UnityEngine;
using VContainer;

// Fails a level when every tracked rope has broken.
[DisallowMultipleComponent]
public sealed class LevelRopeFailureObjective : MonoBehaviour
{
    private IGameStateService gameStateService;
    private RopeGenerator2D[] ropes;
    private int totalRopeCount;
    private int brokenRopeCount;
    private bool levelFailed;

    [Inject]
    // Receives the service used to report level failure.
    public void Construct(IGameStateService gameStateService)
    {
        this.gameStateService = gameStateService;
    }

    // Finds ropes and starts listening for their break events.
    private void OnEnable()
    {
        ropes = GetComponentsInChildren<RopeGenerator2D>(true);
        totalRopeCount = ropes.Length;
        brokenRopeCount = 0;

        foreach (RopeGenerator2D rope in ropes)
        {
            rope.Broken += HandleRopeBroken;

            if (rope.IsBroken)
            {
                brokenRopeCount++;
            }
        }

        TryFailLevel();
    }

    // Stops listening for rope break events.
    private void OnDisable()
    {
        if (ropes == null)
        {
            return;
        }

        foreach (RopeGenerator2D rope in ropes)
        {
            if (rope != null)
            {
                rope.Broken -= HandleRopeBroken;
            }
        }
    }

    // Counts a newly broken rope and checks the objective.
    private void HandleRopeBroken()
    {
        brokenRopeCount++;
        TryFailLevel();
    }

    // Changes the level state after all ropes have broken.
    private void TryFailLevel()
    {
        if (levelFailed || totalRopeCount == 0 || brokenRopeCount < totalRopeCount)
        {
            return;
        }

        if (gameStateService == null)
        {
            Debug.LogError("IGameStateService was not injected into LevelRopeFailureObjective.", this);
            return;
        }

        levelFailed = true;
        gameStateService.ChangeState(GameState.LevelFailed);
    }
}
