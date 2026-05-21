using PrimeTween;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class LevelEndTextAnimation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField, Min(1f)] private float charactersPerSecond = 36f;
    [SerializeField, Min(0f)] private float startDelay = 0.05f;
    [SerializeField] private Ease typewriterEase = Ease.Linear;
    [SerializeField, Min(0f)] private float punchStartDelay = 0.08f;
    [SerializeField, Min(0f)] private float punchDuration = 0.28f;
    [SerializeField, Min(1f)] private float frequency = 4f;
    [SerializeField] private Vector3 punchStrength = new Vector3(0.18f, 0.18f, 0f);

    private Sequence typewriterSequence;
    private Sequence punchSequence;
    private string cachedText;
    private Vector3 initialScale;

    private void Awake()
    {
        if (textMeshProUGUI == null)
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        cachedText = textMeshProUGUI.text;
        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    private void Play()
    {
        Stop();

        textMeshProUGUI.text = cachedText;
        textMeshProUGUI.maxVisibleCharacters = 0;
        textMeshProUGUI.ForceMeshUpdate();
        transform.localScale = initialScale;

        int characterCount = textMeshProUGUI.textInfo.characterCount;
        if (characterCount <= 0)
        {
            return;
        }

        float typewriterDuration = characterCount / charactersPerSecond;
        typewriterSequence = Sequence.Create();
        if (startDelay > 0f)
        {
            typewriterSequence.ChainDelay(startDelay);
        }

        typewriterSequence.Chain(Tween.TextMaxVisibleCharacters(
                textMeshProUGUI,
                startValue: 0,
                endValue: characterCount,
                duration: typewriterDuration,
                ease: typewriterEase));

        if (punchDuration > 0f && punchStrength.sqrMagnitude > 0f)
        {
            punchSequence = Sequence.Create();
            if (punchStartDelay > 0f)
            {
                punchSequence.ChainDelay(punchStartDelay);
            }

            punchSequence.Chain(Tween.PunchScale(transform, punchStrength, punchDuration, frequency: frequency));
        }
    }

    private void Stop()
    {
        if (typewriterSequence.isAlive)
        {
            typewriterSequence.Stop();
        }

        typewriterSequence = default;

        if (punchSequence.isAlive)
        {
            punchSequence.Stop();
        }

        punchSequence = default;

        transform.localScale = initialScale;

        if (textMeshProUGUI != null)
        {
            textMeshProUGUI.maxVisibleCharacters = int.MaxValue;
        }
    }

    private void OnValidate()
    {
        if (textMeshProUGUI == null)
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        charactersPerSecond = Mathf.Max(1f, charactersPerSecond);
        startDelay = Mathf.Max(0f, startDelay);
        punchStartDelay = Mathf.Max(0f, punchStartDelay);
        punchDuration = Mathf.Max(0f, punchDuration);
    }
}
