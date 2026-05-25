using UnityEngine;
using VContainer;

public class UIContinueButton : MonoBehaviour
{
    private ILevelService levelService;

    [Inject]
    public void Construct(ILevelService levelService)
    {
        this.levelService = levelService;
    }

    public void OnContinueButtonClicked()
    {
        if (levelService != null)
        {
            levelService.LoadNextLevel();
        }
    }
}
