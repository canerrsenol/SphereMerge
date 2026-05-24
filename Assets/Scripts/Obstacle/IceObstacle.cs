using TMPro;
using UnityEngine;

public class IceObstacle : ObstacleBaseAbstract, IClickManipulatorObstacle
{
    [SerializeField] private TextMeshPro iceMeltCounterText;
    [SerializeField] private int selectSphereToMelt = 3;
    public bool CanClick => selectSphereToMelt == 0;

    private void OnValidate()
    {
        if (iceMeltCounterText == null)
        {
            iceMeltCounterText = GetComponentInChildren<TextMeshPro>();
        }

        UpdateCounterText();
    }

    public void DecreaseCounter()
    {
        selectSphereToMelt = Mathf.Max(0, selectSphereToMelt - 1);
        UpdateCounterText();
    }

    private void UpdateCounterText()
    {
        if (iceMeltCounterText != null)
        {
            iceMeltCounterText.text = selectSphereToMelt.ToString();
        }
    }
}
