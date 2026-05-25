using System;
using UnityEngine;

// Stores the current game state and notifies interested systems.
public sealed class GameManager : MonoBehaviour, IGameStateService
{
    [SerializeField] private GameState initialState = GameState.Initializing;

    private GameState currentState;

    public GameState CurrentState => currentState;
    public event Action<GameState> StateChanged;

    // Applies the starting state when the component is created.
    private void Awake()
    {
        currentState = initialState;
    }

    // Changes the game state and publishes the new value.
    public void ChangeState(GameState newState)
    {
        if (currentState == newState || (IsTerminalState(currentState) && IsTerminalState(newState)))
        {
            return;
        }

        currentState = newState;
        StateChanged?.Invoke(currentState);
    }

    // Exposes state changes for Unity event calls.
    public void ChangeGameState(GameState newState)
    {
        ChangeState(newState);
    }

    // Checks whether the game is currently in a given state.
    public bool IsState(GameState state)
    {
        return currentState == state;
    }

    // Returns true for states that finish a level.
    private static bool IsTerminalState(GameState state)
    {
        return state == GameState.LevelCompleted || state == GameState.LevelFailed;
    }
}
