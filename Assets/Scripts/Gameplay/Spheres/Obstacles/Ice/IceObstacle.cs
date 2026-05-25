using TMPro;
using UnityEngine;

// Blocks a sphere until enough sphere selections melt the ice.
public class IceObstacle : ObstacleBaseAbstract, IClickManipulatorObstacle
{
    [SerializeField] private TextMeshPro iceMeltCounterText;
    [SerializeField] private SpriteRenderer iceImage;
    [SerializeField] private int selectSphereToMelt = 3;
    public bool CanClick => selectSphereToMelt == 0;

    // Finds visual parts and displays the current melt count.
    private void Awake()
    {
        CacheReferences();
        UpdateVisuals();
    }

    // Starts listening for player sphere selections.
    private void OnEnable()
    {
        EventBus.Subscribe<SphereSelectedEvent>(HandleSphereSelected);
    }

    // Stops listening for player sphere selections.
    private void OnDisable()
    {
        EventBus.Unsubscribe<SphereSelectedEvent>(HandleSphereSelected);
    }

    // Keeps the counter valid and refreshes editor visuals.
    private void OnValidate()
    {
        selectSphereToMelt = Mathf.Max(0, selectSphereToMelt);
        CacheReferences();
        UpdateVisuals();
    }

    // Removes one remaining ice layer after a selection.
    public void DecreaseCounter()
    {
        if (CanClick)
        {
            return;
        }

        selectSphereToMelt = Mathf.Max(0, selectSphereToMelt - 1);
        UpdateVisuals();
    }

    // Reacts to a selected sphere by melting one ice layer.
    private void HandleSphereSelected(SphereSelectedEvent selectionEvent)
    {
        DecreaseCounter();
    }

    // Finds the optional text and ice image components.
    private void CacheReferences()
    {
        if (iceMeltCounterText == null)
        {
            iceMeltCounterText = GetComponentInChildren<TextMeshPro>(true);
        }

        if (iceImage == null)
        {
            iceImage = GetComponentInChildren<SpriteRenderer>(true);
        }
    }

    // Shows the ice and counter until melting is complete.
    private void UpdateVisuals()
    {
        if (iceMeltCounterText != null)
        {
            iceMeltCounterText.text = selectSphereToMelt.ToString();
            iceMeltCounterText.enabled = !CanClick;
        }

        if (iceImage != null)
        {
            iceImage.enabled = !CanClick;
        }
    }
}
