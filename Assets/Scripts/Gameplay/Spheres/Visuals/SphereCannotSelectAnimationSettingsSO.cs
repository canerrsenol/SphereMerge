using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "SphereCannotSelectAnimationSettings", menuName = "Sphere Merge/Sphere Cannot Select Animation Settings")]
public sealed class SphereCannotSelectAnimationSettingsSO : ScriptableObject
{
    [SerializeField, Min(0.01f)] private float duration = 0.18f;
    [SerializeField, Min(0f)] private float shakeDistance = 0.08f;
    [SerializeField, Min(0f)] private float verticalShakeDistance = 0.05f;
    [SerializeField, Min(1)] private int shakeStepCount = 3;
    [SerializeField] private Ease ease = Ease.OutSine;

    public float Duration => duration;
    public float ShakeDistance => shakeDistance;
    public float VerticalShakeDistance => verticalShakeDistance;
    public int ShakeStepCount => shakeStepCount;
    public Ease Ease => ease;

    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);
        shakeDistance = Mathf.Max(0f, shakeDistance);
        verticalShakeDistance = Mathf.Max(0f, verticalShakeDistance);
        shakeStepCount = Mathf.Max(1, shakeStepCount);
    }
}
