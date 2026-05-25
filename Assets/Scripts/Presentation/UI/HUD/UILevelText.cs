using TMPro;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UILevelText : MonoBehaviour
{
    private TextMeshProUGUI levelText;
    private ILevelService levelService;
    private bool subscribed;

    [Inject]
    public void Construct(ILevelService levelService)
    {
        this.levelService = levelService;
    }
    
    private void Awake()
    {
        levelText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        Subscribe();

        if (levelService != null)
        {
            SetLevelText(levelService.CurrentLevelIndex);
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (subscribed || levelService == null)
        {
            return;
        }

        levelService.LevelLoaded += SetLevelText;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || levelService == null)
        {
            return;
        }

        levelService.LevelLoaded -= SetLevelText;
        subscribed = false;
    }

    private void SetLevelText(int levelIndex)
    {
        levelText.text = $"Level {levelIndex + 1}";
    }
}
