using UnityEngine;
using VContainer;

public sealed class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    private IGameStateService gameStateService;
    private ILevelService levelService;
    private bool subscribed;

    [Inject]
    public void Construct(IGameStateService gameStateService, ILevelService levelService)
    {
        this.gameStateService = gameStateService;
        this.levelService = levelService;
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        Subscribe();

        if (gameStateService != null)
        {
            HandleGameStateChanged(gameStateService.CurrentState);
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void OnRetryLevelButtonPressed()
    {
        if (gameStateService == null || levelService == null)
        {
            Debug.LogWarning("UIManager dependencies are not ready.", this);
            return;
        }

        gameStateService.ChangeState(GameState.Playing);
        levelService.LoadCurrentLevel();
    }

    public void OnNextLevelButtonPressed()
    {
        if (gameStateService == null || levelService == null)
        {
            Debug.LogWarning("UIManager dependencies are not ready.", this);
            return;
        }

        gameStateService.ChangeState(GameState.Playing);
        levelService.LoadNextLevel();
    }

    private void Subscribe()
    {
        if (subscribed || gameStateService == null)
        {
            return;
        }

        gameStateService.StateChanged += HandleGameStateChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || gameStateService == null)
        {
            return;
        }

        gameStateService.StateChanged -= HandleGameStateChanged;
        subscribed = false;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Initializing:
            case GameState.Playing:
                SetPanels(false, false, false);
                break;

            case GameState.Paused:
                SetPanels(true, false, false);
                break;

            case GameState.LevelCompleted:
                SetPanels(true, false, true);
                break;

            case GameState.LevelFailed:
                SetPanels(true, true, false);
                break;

            default:
                SetPanels(false, false, false);
                break;
        }
    }

    private void SetPanels(bool background, bool gameOver, bool win)
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
