using TMPro;
using UnityEngine;

public class IceObstacle : ObstacleBaseAbstract, IClickManipulatorObstacle
{
    [SerializeField] private TextMeshPro iceMeltCounterText;
    [SerializeField] private SpriteRenderer iceImage;
    [SerializeField] private int selectSphereToMelt = 3;
    public bool CanClick => selectSphereToMelt == 0;

    private void OnEnable()
    {
        EventBus.Subscribe<SphereSelectedEvent>(HandleSphereSelected);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SphereSelectedEvent>(HandleSphereSelected);
    }

    private void OnValidate()
    {
        if (iceMeltCounterText == null)
        {
            iceMeltCounterText = GetComponentInChildren<TextMeshPro>();
        }
        if (iceImage == null)
        {
            iceImage = GetComponentInChildren<SpriteRenderer>();
        }

        UpdateCounterText();
    }

    public void DecreaseCounter()
    {
        selectSphereToMelt = Mathf.Max(0, selectSphereToMelt - 1);
        UpdateCounterText();

        if (selectSphereToMelt == 0)
        {
            iceImage.enabled = false;
            iceMeltCounterText.enabled = false;
        }
    }

    private void HandleSphereSelected(SphereSelectedEvent selectionEvent)
    {
        DecreaseCounter();
    }

    private void UpdateCounterText()
    {
        if (iceMeltCounterText != null)
        {
            iceMeltCounterText.text = selectSphereToMelt.ToString();
        }
    }
}
