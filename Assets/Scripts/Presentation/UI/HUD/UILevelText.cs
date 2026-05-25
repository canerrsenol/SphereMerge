using TMPro;
using UnityEngine;
using VContainer;

// Displays the number of the active level on the HUD.
[RequireComponent(typeof(TextMeshProUGUI))]
public class UILevelText : MonoBehaviour
{
    private TextMeshProUGUI levelText;
    private ILevelService levelService;
    private bool subscribed;

    [Inject]
    // Receives the service that reports loaded levels.
    public void Construct(ILevelService levelService)
    {
        this.levelService = levelService;
    }
    // Finds the text component used for level display.
    private void Awake()
    {
        levelText = GetComponent<TextMeshProUGUI>();
    }

    // Tries to start listening for level loading.
    private void OnEnable()
    {
        Subscribe();
    }

    // Subscribes after injection and displays the active level.
    private void Start()
    {
        Subscribe();

        if (levelService != null)
        {
            SetLevelText(levelService.CurrentLevelIndex);
        }
    }

    // Stops listening for level loading.
    private void OnDisable()
    {
        Unsubscribe();
    }

    // Starts listening to level changes once the service is ready.
    private void Subscribe()
    {
        if (subscribed || levelService == null)
        {
            return;
        }

        levelService.LevelLoaded += SetLevelText;
        subscribed = true;
    }

    // Stops listening to level changes.
    private void Unsubscribe()
    {
        if (!subscribed || levelService == null)
        {
            return;
        }

        levelService.LevelLoaded -= SetLevelText;
        subscribed = false;
    }

    // Writes a human-friendly level number into the label.
    private void SetLevelText(int levelIndex)
    {
        levelText.text = $"Level {levelIndex + 1}";
    }
}
