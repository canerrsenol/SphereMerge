using UnityEngine;
using VContainer;

[DisallowMultipleComponent]
public sealed class LevelRopeFailureObjective : MonoBehaviour
{
    private IGameStateService gameStateService;
    private RopeGenerator2D[] ropes;
    private int totalRopeCount;
    private int brokenRopeCount;
    private bool levelFailed;

    [Inject]
    public void Construct(IGameStateService gameStateService)
    {
        this.gameStateService = gameStateService;
    }

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

    private void HandleRopeBroken()
    {
        brokenRopeCount++;
        TryFailLevel();
    }

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
