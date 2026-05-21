using System;

public interface IGameStateService
{
    GameState CurrentState { get; }
    event Action<GameState> StateChanged;
    void ChangeState(GameState newState);
    bool IsState(GameState state);
}
