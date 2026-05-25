using UnityEngine;
using VContainer;

public class UIRetryButton : MonoBehaviour
{
private ILevelService levelService;

    [Inject]
    public void Construct(ILevelService levelService)
    {
        this.levelService = levelService;
    }

    public void OnRetryButtonClicked()
    {
        if (levelService != null)
        {
            levelService.LoadCurrentLevel();
        }
    }
}
