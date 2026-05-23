using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "SphereIntroAnimationSettings", menuName = "Sphere Merge/Sphere Intro Animation Settings")]
public class SphereIntroAnimationSettingsSO : ScriptableObject
{
    [SerializeField, Min(0.01f)] private float duration = 0.25f;
    [SerializeField, Min(0f)] private float stagger = 0.03f;
    [SerializeField] private Ease ease = Ease.OutBack;

    public float Duration => duration;
    public float Stagger => stagger;
    public Ease Ease => ease;

    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);
        stagger = Mathf.Max(0f, stagger);
    }
}
