using PrimeTween;
using TMPro;
using UnityEngine;

// Reveals level end text with typing and scale feedback.
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

    // Caches the original label text and scale.
    private void Awake()
    {
        if (textMeshProUGUI == null)
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        cachedText = textMeshProUGUI.text;
        initialScale = transform.localScale;
    }

    // Plays the animation whenever this text becomes visible.
    private void OnEnable()
    {
        Play();
    }

    // Stops animations when the text is hidden.
    private void OnDisable()
    {
        Stop();
    }

    // Starts the typewriter reveal and optional scale punch.
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

    // Stops active animations and restores normal visuals.
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

    // Keeps animation settings valid in the inspector.
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
