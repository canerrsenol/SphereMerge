using PrimeTween;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GlassSphereVisual2D : MonoBehaviour
{
    [Header("Cannot Select Animation")]
    [Min(0.01f)] [SerializeField] private float duration = 0.18f;
    [Min(0f)] [SerializeField] private float shakeDistance = 0.08f;
    [Min(0f)] [SerializeField] private float verticalShakeDistance = 0.05f;
    [Min(1)] [SerializeField] private int shakeStepCount = 3;
    [SerializeField] private Ease ease = Ease.OutSine;

    private Sequence cannotSelectSequence;
    private Vector3 initialLocalPosition;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
    }

    private void OnDestroy()
    {
        if (cannotSelectSequence.isAlive)
        {
            cannotSelectSequence.Stop();
        }

        cannotSelectSequence = default;
    }

    public void PlayCannotSelectAnimation()
    {
        if (cannotSelectSequence.isAlive)
        {
            cannotSelectSequence.Stop();
        }

        transform.localPosition = initialLocalPosition;

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

    private Vector3 GetRandomShakeOffset()
    {
        Vector2 direction = Random.insideUnitCircle;
        if (direction.sqrMagnitude <= float.Epsilon)
        {
            direction = Vector2.right;
        }

        direction.Normalize();
        return new Vector3(direction.x * shakeDistance, direction.y * verticalShakeDistance, 0f);
    }
}
