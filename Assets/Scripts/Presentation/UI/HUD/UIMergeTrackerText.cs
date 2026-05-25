using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class UIMergeTrackerText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mergeCountText;

    private void Awake()
    {
        CacheTextComponent();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<MergeProgressChangedEvent>(HandleProgressChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<MergeProgressChangedEvent>(HandleProgressChanged);
    }

    private void Reset()
    {
        CacheTextComponent();
    }

    private void OnValidate()
    {
        CacheTextComponent();
    }

    private void HandleProgressChanged(MergeProgressChangedEvent progressEvent)
    {
        CacheTextComponent();
        if (mergeCountText != null)
        {
            mergeCountText.text = $"{progressEvent.CompletedMergeCount}/{progressEvent.TotalMergeCount}";
        }
    }

    private void CacheTextComponent()
    {
        if (mergeCountText == null)
        {
            mergeCountText = GetComponent<TextMeshProUGUI>();
        }
    }
}
