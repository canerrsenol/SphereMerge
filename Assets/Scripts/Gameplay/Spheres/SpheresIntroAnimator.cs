using PrimeTween;
using UnityEngine;

// Plays the starting animation for the spheres in a level grid.
[DisallowMultipleComponent]
[RequireComponent(typeof(SpheresManager))]
[RequireComponent(typeof(SphereColumnActivationController))]
public sealed class SpheresIntroAnimator : MonoBehaviour
{
    private const float DefaultIntroDuration = 0.25f;
    private const float DefaultIntroStagger = 0.03f;
    private const Ease DefaultIntroEase = Ease.OutBack;

    [SerializeField] private SpheresManager spheresManager;
    [SerializeField] private SphereColumnActivationController activationController;
    [SerializeField] private SphereIntroAnimationSettingsSO animationSettings;

    private Sequence introSequence;

    // Finds components needed to animate and activate spheres.
    private void Awake()
    {
        CacheReferences();
    }

    // Plays the level opening animation after grid setup.
    private void Start()
    {
        PlayIntroAnimation();
    }

    // Stops any running intro animation when this object is destroyed.
    private void OnDestroy()
    {
        if (introSequence.isAlive)
        {
            introSequence.Stop();
        }

        introSequence = default;
    }

    // Refreshes component references when this animator is added.
    private void Reset()
    {
        CacheReferences();
    }

    // Animates all spheres and enables selection after the animation.
    private void PlayIntroAnimation()
    {
        if (spheresManager == null || !spheresManager.IsGridSizeValid)
        {
            ActivateInitialSpheres();
            return;
        }

        introSequence = Sequence.Create();

        float duration = animationSettings != null ? animationSettings.Duration : DefaultIntroDuration;
        float stagger = animationSettings != null ? animationSettings.Stagger : DefaultIntroStagger;
        Ease ease = animationSettings != null ? animationSettings.Ease : DefaultIntroEase;
        Vector2Int gridSize = spheresManager.GridSize;
        bool hasIntroTween = false;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                GlassSphere2D sphere = spheresManager.GetSphere(new Vector2Int(x, y));
                if (sphere == null)
                {
                    continue;
                }

                Transform sphereTransform = sphere.transform;
                Vector3 targetScale = sphereTransform.localScale;
                sphereTransform.localScale = Vector3.zero;

                introSequence.Group(Tween.Scale(
                    sphereTransform,
                    targetScale,
                    duration,
                    ease,
                    startDelay: (x + y) * stagger));
                hasIntroTween = true;
            }
        }

        if (hasIntroTween)
        {
            introSequence.ChainCallback(ActivateInitialSpheres);
            return;
        }

        ActivateInitialSpheres();
    }

    // Starts selection in each column after the intro is complete.
    private void ActivateInitialSpheres()
    {
        activationController?.ActivateInitialSpheres();
    }

    // Finds local components used by this animator.
    private void CacheReferences()
    {
        if (spheresManager == null)
        {
            spheresManager = GetComponent<SpheresManager>();
        }

        if (activationController == null)
        {
            activationController = GetComponent<SphereColumnActivationController>();
        }
    }
}
