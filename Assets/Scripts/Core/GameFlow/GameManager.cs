using System;
using UnityEngine;

public sealed class GameManager : MonoBehaviour, IGameStateService
{
    [SerializeField] private GameState initialState = GameState.Initializing;

    private GameState currentState;

    public GameState CurrentState => currentState;
    public event Action<GameState> StateChanged;

    private void Awake()
    {
        currentState = initialState;
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
        StateChanged?.Invoke(currentState);
    }

    public void ChangeGameState(GameState newState)
    {
        ChangeState(newState);
    }

    public bool IsState(GameState state)
    {
        return currentState == state;
    }
}
