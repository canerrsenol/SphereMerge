using PrimeTween;
using UnityEngine;
using VContainer;

// Displays gameplay panels when the current game state changes.
public sealed class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField, Min(0f)] private float panelOpenDelay = 1f;

    private IGameStateService gameStateService;
    private ILevelService levelService;
    private Sequence panelTransitionSequence;
    private bool subscribed;

    [Inject]
    // Receives game and level services needed by the UI.
    public void Construct(IGameStateService gameStateService, ILevelService levelService)
    {
        this.gameStateService = gameStateService;
        this.levelService = levelService;
    }

    // Tries to start listening for game state changes.
    private void OnEnable()
    {
        Subscribe();
    }

    // Subscribes after injection and shows the current state.
    private void Start()
    {
        Subscribe();

        if (gameStateService != null)
        {
            HandleGameStateChanged(gameStateService.CurrentState);
        }
    }

    // Stops listening and cancels delayed panel changes.
    private void OnDisable()
    {
        Unsubscribe();
        StopPanelTransition();
    }

    // Reloads the level after the retry button is pressed.
    public void OnRetryLevelButtonPressed()
    {
        if (gameStateService == null || levelService == null)
        {
            Debug.LogWarning("UIManager dependencies are not ready.", this);
            return;
        }

        levelService.LoadCurrentLevel();
    }

    // Loads the next level after the continue button is pressed.
    public void OnNextLevelButtonPressed()
    {
        if (gameStateService == null || levelService == null)
        {
            Debug.LogWarning("UIManager dependencies are not ready.", this);
            return;
        }

        levelService.LoadNextLevel();
    }

    // Starts listening to the game state service once available.
    private void Subscribe()
    {
        if (subscribed || gameStateService == null)
        {
            return;
        }

        gameStateService.StateChanged += HandleGameStateChanged;
        subscribed = true;
    }

    // Stops listening to game state changes.
    private void Unsubscribe()
    {
        if (!subscribed || gameStateService == null)
        {
            return;
        }

        gameStateService.StateChanged -= HandleGameStateChanged;
        subscribed = false;
    }

    // Selects visible panels for the new game state.
    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Initializing:
            case GameState.Playing:
                SetPanels(false, false, false, 0f);
                break;

            case GameState.Paused:
                SetPanels(true, false, false, panelOpenDelay);
                break;

            case GameState.LevelCompleted:
                SetPanels(true, false, true, panelOpenDelay);
                break;

            case GameState.LevelFailed:
                SetPanels(true, true, false, panelOpenDelay);
                break;

            default:
                SetPanels(false, false, false, 0f);
                break;
        }
    }

    // Applies panel visibility now or after a delay.
    private void SetPanels(bool background, bool gameOver, bool win, float delay)
    {
        StopPanelTransition();

        if (delay <= 0f)
        {
            ApplyPanels(background, gameOver, win);
            return;
        }

        panelTransitionSequence = Sequence.Create()
            .ChainDelay(delay)
            .ChainCallback(() => ApplyPanels(background, gameOver, win));
    }

    // Cancels a delayed panel transition.
    private void StopPanelTransition()
    {
        if (!panelTransitionSequence.isAlive)
        {
            return;
        }

        panelTransitionSequence.Stop();
    }

    // Enables only the panels needed by the current screen.
    private void ApplyPanels(bool background, bool gameOver, bool win)
    {
        if (backgroundPanel != null)
        {
            backgroundPanel.SetActive(background);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(gameOver);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(win);
        }
    }
}
