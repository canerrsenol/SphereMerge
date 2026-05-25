using PrimeTween;
using UnityEngine;

// Scales a UI element in and then plays a soft loop animation.
[DisallowMultipleComponent]
public class ScaleAnimation : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float startScaleValue = 0.3f;
    [SerializeField, Min(0.01f)] private float enterDuration = 0.35f;
    [SerializeField] private Ease enterEase = Ease.OutBack;
    [SerializeField, Min(1f)] private float loopScaleMultiplier = 1.08f;
    [SerializeField, Min(0.01f)] private float loopHalfCycleDuration = 0.35f;
    [SerializeField] private Ease loopEase = Ease.InOutSine;
    [SerializeField] private bool resetScaleOnDisable = true;

    private Sequence enterSequence;
    private Tween loopTween;
    private Vector3 defaultScale;

    private Transform Target => target != null ? target : transform;

    // Saves the normal target scale before animation begins.
    private void Awake()
    {
        defaultScale = Target.localScale;
    }

    // Plays the scale animation when this object becomes visible.
    private void OnEnable()
    {
        Play();
    }

    // Stops animation when this object is hidden.
    private void OnDisable()
    {
        Stop();
    }

    // Plays the entrance animation before the loop starts.
    private void Play()
    {
        Stop();

        var tweenTarget = Target;
        tweenTarget.localScale = defaultScale * startScaleValue;

        enterSequence = Sequence.Create()
            .Chain(Tween.Scale(
                tweenTarget,
                startValue: defaultScale * startScaleValue,
                endValue: defaultScale,
                duration: enterDuration,
                ease: enterEase))
            .ChainCallback(this, StartLoopFromCallback, warnIfTargetDestroyed: false);
    }

    // Starts looping through a tween callback.
    private static void StartLoopFromCallback(ScaleAnimation animation)
    {
        animation.StartLoop();
    }

    // Repeats a small scale pulse while this object is active.
    private void StartLoop()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        var tweenTarget = Target;
        tweenTarget.localScale = defaultScale;
        loopTween = Tween.Scale(
            tweenTarget,
            defaultScale * loopScaleMultiplier,
            loopHalfCycleDuration,
            loopEase,
            cycles: -1,
            cycleMode: CycleMode.Yoyo);
    }

    // Stops all scale animations and optionally restores scale.
    private void Stop()
    {
        if (enterSequence.isAlive)
        {
            enterSequence.Stop();
        }

        enterSequence = default;

        if (loopTween.isAlive)
        {
            loopTween.Stop();
        }

        loopTween = default;

        if (resetScaleOnDisable)
        {
            Target.localScale = defaultScale;
        }
    }

    // Keeps animation values valid in the inspector.
    private void OnValidate()
    {
        startScaleValue = Mathf.Max(0f, startScaleValue);
        enterDuration = Mathf.Max(0.01f, enterDuration);
        loopScaleMultiplier = Mathf.Max(1f, loopScaleMultiplier);
        loopHalfCycleDuration = Mathf.Max(0.01f, loopHalfCycleDuration);
    }
}
