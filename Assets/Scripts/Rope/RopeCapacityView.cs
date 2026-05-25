using PrimeTween;
using TMPro;
using UnityEngine;

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

    private void Awake()
    {
        EnsureInitialized();
        ApplyNormalVisuals();
    }

    private void OnDestroy()
    {
        StopWarningAnimation();
    }

    private void Reset()
    {
        CacheText();
    }

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

    private void CacheText()
    {
        if (counterText == null)
        {
            counterText = GetComponent<TextMeshPro>();
        }
    }

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
