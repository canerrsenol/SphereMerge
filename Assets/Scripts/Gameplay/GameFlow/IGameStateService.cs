using System;

// Provides access to the current game state and state changes.
public interface IGameStateService
{
    GameState CurrentState { get; }
    event Action<GameState> StateChanged;
    // Changes the current state when the transition is allowed.
    void ChangeState(GameState newState);
    // Returns true when the current state matches the given state.
    bool IsState(GameState state);
}
