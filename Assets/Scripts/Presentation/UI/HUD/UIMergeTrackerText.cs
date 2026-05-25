using TMPro;
using UnityEngine;

// Displays completed and required merges for the current level.
[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class UIMergeTrackerText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mergeCountText;

    // Finds the HUD label when gameplay begins.
    private void Awake()
    {
        CacheTextComponent();
    }

    // Starts listening for merge progress changes.
    private void OnEnable()
    {
        EventBus.Subscribe<MergeProgressChangedEvent>(HandleProgressChanged);
    }

    // Stops listening for merge progress changes.
    private void OnDisable()
    {
        EventBus.Unsubscribe<MergeProgressChangedEvent>(HandleProgressChanged);
    }

    // Finds the text component when this component is added.
    private void Reset()
    {
        CacheTextComponent();
    }

    // Refreshes the text reference in the inspector.
    private void OnValidate()
    {
        CacheTextComponent();
    }

    // Displays the latest merge progress value.
    private void HandleProgressChanged(MergeProgressChangedEvent progressEvent)
    {
        CacheTextComponent();
        if (mergeCountText != null)
        {
            mergeCountText.text = $"{progressEvent.CompletedMergeCount}/{progressEvent.TotalMergeCount}";
        }
    }

    // Finds the text component when it was not assigned.
    private void CacheTextComponent()
    {
        if (mergeCountText == null)
        {
            mergeCountText = GetComponent<TextMeshProUGUI>();
        }
    }
}
