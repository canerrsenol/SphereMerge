using PrimeTween;
using UnityEngine;

// Stores animation values for merging spheres.
[CreateAssetMenu(fileName = "SphereMergeAnimationSettings", menuName = "Sphere Merge/Sphere Merge Animation Settings")]
public sealed class SphereMergeAnimationSettingsSO : ScriptableObject
{
    [SerializeField, Min(0.01f)] private float duration = 0.18f;
    [SerializeField] private Ease positionEase = Ease.InBack;
    [SerializeField] private Ease scaleEase = Ease.OutSine;
    [SerializeField, Min(0f)] private float targetScale = 0.35f;

    public float Duration => duration;
    public Ease PositionEase => positionEase;
    public Ease ScaleEase => scaleEase;
    public float TargetScale => targetScale;

    // Keeps merge animation values inside valid ranges.
    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);
        targetScale = Mathf.Max(0f, targetScale);
    }
}
