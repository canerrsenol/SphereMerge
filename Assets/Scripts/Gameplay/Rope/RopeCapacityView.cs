using PrimeTween;
using TMPro;
using UnityEngine;

// Shows remaining rope capacity and warns before the rope breaks.
[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshPro))]
public sealed class RopeCapacityView : MonoBehaviour
{
    [SerializeField] private TextMeshPro counterText;

    [Header("Normal")]
    [SerializeField] private Color normalColor = Color.white;

    [Header("Warning")]
    [SerializeField] private Color warningColor = Color.red;
    [Min(1f)]
    [SerializeField] private float warningScaleMultiplier = 1.25f;
    [Min(0.01f)]
    [SerializeField] private float warningHalfCycleDuration = 0.2f;
    [SerializeField] private Ease warningEase = Ease.InOutSine;

    private Tween warningScaleTween;
    private Tween warningColorTween;
    private Vector3 normalScale;
    private int displayedCapacity = -1;
    private bool isHidden;
    private bool isInitialized;

    // Prepares the text and normal visual appearance.
    private void Awake()
    {
        EnsureInitialized();
        ApplyNormalVisuals();
    }

    // Stops active warning animations when the view is removed.
    private void OnDestroy()
    {
        StopWarningAnimation();
    }

    // Finds the text component after adding this view.
    private void Reset()
    {
        CacheText();
    }

    // Keeps animation values valid in the inspector.
    private void OnValidate()
    {
        warningScaleMultiplier = Mathf.Max(1f, warningScaleMultiplier);
        warningHalfCycleDuration = Mathf.Max(0.01f, warningHalfCycleDuration);
        CacheText();

        if (!Application.isPlaying && counterText != null)
        {
            counterText.color = normalColor;
        }
    }

    // Displays a new remaining capacity value and its warning state.
    public void SetRemainingCapacity(int remainingCapacity)
    {
        EnsureInitialized();

        if (counterText == null || isHidden || displayedCapacity == remainingCapacity)
        {
            return;
        }

        displayedCapacity = remainingCapacity;
        counterText.text = remainingCapacity.ToString();

        if (!Application.isPlaying)
        {
            return;
        }

        StopWarningAnimation();

        if (remainingCapacity == 1)
        {
            PlayWarningAnimation();
        }
    }

    // Enables the counter and returns it to normal visuals.
    public void Show()
    {
        EnsureInitialized();
        isHidden = false;
        displayedCapacity = -1;
        StopWarningAnimation();

        if (counterText != null)
        {
            counterText.enabled = true;
            counterText.color = normalColor;
        }
    }

    // Hides the counter after the rope breaks.
    public void Hide()
    {
        EnsureInitialized();
        isHidden = true;
        StopWarningAnimation();

        if (counterText != null)
        {
            counterText.enabled = false;
        }
    }

    // Pulses the counter when only one capacity point remains.
    private void PlayWarningAnimation()
    {
        warningScaleTween = Tween.Scale(
            transform,
            normalScale * warningScaleMultiplier,
            warningHalfCycleDuration,
            warningEase,
            cycles: 2,
            cycleMode: CycleMode.Yoyo);

        warningColorTween = Tween.Custom(
            this,
            0f,
            1f,
            warningHalfCycleDuration,
            static (view, amount) => view.counterText.color = Color.Lerp(view.normalColor, view.warningColor, amount),
            warningEase,
            cycles: 2,
            cycleMode: CycleMode.Yoyo);
    }

    // Stops warning tweens and restores normal visuals.
    private void StopWarningAnimation()
    {
        if (warningScaleTween.isAlive)
        {
            warningScaleTween.Stop();
        }

        warningScaleTween = default;

        if (warningColorTween.isAlive)
        {
            warningColorTween.Stop();
        }

        warningColorTween = default;
        ApplyNormalVisuals();
    }

    // Finds the counter text if it was not assigned.
    private void CacheText()
    {
        if (counterText == null)
        {
            counterText = GetComponent<TextMeshPro>();
        }
    }

    // Saves the starting scale before the view is used.
    private void EnsureInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        CacheText();
        normalScale = transform.localScale;
        isInitialized = true;
    }

    // Restores normal scale and text color.
    private void ApplyNormalVisuals()
    {
        if (!isInitialized)
        {
            return;
        }

        transform.localScale = normalScale;

        if (counterText != null)
        {
            counterText.color = normalColor;
        }
    }
}
