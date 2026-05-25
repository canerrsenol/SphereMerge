using PrimeTween;
using UnityEngine;

// Plays feedback animations for a glass sphere visual.
[DisallowMultipleComponent]
public sealed class GlassSphereVisual2D : MonoBehaviour
{
    private const float DefaultDuration = 0.18f;
    private const float DefaultShakeDistance = 0.08f;
    private const float DefaultVerticalShakeDistance = 0.05f;
    private const int DefaultShakeStepCount = 3;
    private const Ease DefaultEase = Ease.OutSine;

    [Header("Cannot Select Animation")]
    [SerializeField] private SphereCannotSelectAnimationSettingsSO cannotSelectAnimationSettings;

    private Sequence cannotSelectSequence;
    private Vector3 initialLocalPosition;

    // Saves the normal position used after an animation.
    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
    }

    // Stops a running feedback animation before destruction.
    private void OnDestroy()
    {
        if (cannotSelectSequence.isAlive)
        {
            cannotSelectSequence.Stop();
        }

        cannotSelectSequence = default;
    }

    // Shakes the sphere to show that it cannot be selected.
    public void PlayCannotSelectAnimation()
    {
        if (cannotSelectSequence.isAlive)
        {
            cannotSelectSequence.Stop();
        }

        transform.localPosition = initialLocalPosition;

        float duration = cannotSelectAnimationSettings != null ? cannotSelectAnimationSettings.Duration : DefaultDuration;
        int shakeStepCount = cannotSelectAnimationSettings != null
            ? cannotSelectAnimationSettings.ShakeStepCount
            : DefaultShakeStepCount;
        Ease ease = cannotSelectAnimationSettings != null ? cannotSelectAnimationSettings.Ease : DefaultEase;
        float stepDuration = duration / (shakeStepCount + 1);
        cannotSelectSequence = Sequence.Create();

        for (int i = 0; i < shakeStepCount; i++)
        {
            cannotSelectSequence.Chain(Tween.LocalPosition(
                transform,
                initialLocalPosition + GetRandomShakeOffset(),
                stepDuration,
                ease));
        }

        cannotSelectSequence.Chain(Tween.LocalPosition(transform, initialLocalPosition, stepDuration, ease));
    }

    // Creates a small random shake direction for feedback.
    private Vector3 GetRandomShakeOffset()
    {
        float shakeDistance = cannotSelectAnimationSettings != null
            ? cannotSelectAnimationSettings.ShakeDistance
            : DefaultShakeDistance;
        float verticalShakeDistance = cannotSelectAnimationSettings != null
            ? cannotSelectAnimationSettings.VerticalShakeDistance
            : DefaultVerticalShakeDistance;
        Vector2 direction = Random.insideUnitCircle;
        if (direction.sqrMagnitude <= float.Epsilon)
        {
            direction = Vector2.right;
        }

        direction.Normalize();
        return new Vector3(direction.x * shakeDistance, direction.y * verticalShakeDistance, 0f);
    }
}
