using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SpheresMergeManager : MonoBehaviour
    , ISpheresMergeManagerService
{
    private const int MergeCount = 3;
    private const float DefaultMergeDuration = 0.18f;
    private const Ease DefaultMergePositionEase = Ease.InBack;
    private const Ease DefaultMergeScaleEase = Ease.OutSine;
    private const float DefaultMergeTargetScale = 0.35f;

    [Header("Animation")]
    [SerializeField] private SphereMergeAnimationSettingsSO mergeAnimationSettings;

    private readonly List<SphereContact> contacts = new List<SphereContact>();
    private readonly List<GlassSphere2D> mergeCandidates = new List<GlassSphere2D>(MergeCount);

    public void ReportSphereContact(GlassSphere2D first, GlassSphere2D second)
    {
        if (!CanUseContact(first, second))
        {
            return;
        }

        SphereContact contact = new SphereContact(first, second);
        if (!ContainsContact(contact))
        {
            contacts.Add(contact);
        }

        TryMerge(first);
        TryMerge(second);
    }

    public void ReportSphereContactEnded(GlassSphere2D first, GlassSphere2D second)
    {
        RemoveContact(first, second);
    }

    private void TryMerge(GlassSphere2D seed)
    {
        if (!CanMerge(seed))
        {
            return;
        }

        FindFirstThreeConnectedSpheres(seed);

        if (mergeCandidates.Count < MergeCount)
        {
            return;
        }

        GlassSphere2D[] group = mergeCandidates.ToArray();
        StartMerge(group);
    }

    private void FindFirstThreeConnectedSpheres(GlassSphere2D seed)
    {
        mergeCandidates.Clear();
        mergeCandidates.Add(seed);

        SphereColors color = seed.SphereColor;
        bool addedSphere;

        do
        {
            addedSphere = false;

            for (int i = 0; i < contacts.Count && mergeCandidates.Count < MergeCount; i++)
            {
                SphereContact contact = contacts[i];
                if (!contact.HasColor(color) || !contact.TouchesAny(mergeCandidates))
                {
                    continue;
                }

                addedSphere |= TryAddCandidate(contact.First, color);
                if (mergeCandidates.Count >= MergeCount)
                {
                    break;
                }

                addedSphere |= TryAddCandidate(contact.Second, color);
            }
        }
        while (addedSphere && mergeCandidates.Count < MergeCount);
    }

    private bool TryAddCandidate(GlassSphere2D sphere, SphereColors color)
    {
        if (!CanMerge(sphere) || sphere.SphereColor != color || mergeCandidates.Contains(sphere))
        {
            return false;
        }

        mergeCandidates.Add(sphere);
        return true;
    }

    private void StartMerge(GlassSphere2D[] group)
    {
        float duration = mergeAnimationSettings != null ? mergeAnimationSettings.Duration : DefaultMergeDuration;
        Ease positionEase = mergeAnimationSettings != null ? mergeAnimationSettings.PositionEase : DefaultMergePositionEase;
        Ease scaleEase = mergeAnimationSettings != null ? mergeAnimationSettings.ScaleEase : DefaultMergeScaleEase;
        float targetScale = mergeAnimationSettings != null ? mergeAnimationSettings.TargetScale : DefaultMergeTargetScale;
        Vector3 mergePosition = GetAveragePosition(group);
        Sequence sequence = Sequence.Create();

        for (int i = 0; i < group.Length; i++)
        {
            GlassSphere2D sphere = group[i];
            if (sphere == null)
            {
                continue;
            }

            sphere.SetSphereState(SphereState.Merged);
            RemoveContactsWith(sphere);

            Transform sphereTransform = sphere.transform;
            sequence.Group(Tween.PositionX(sphereTransform, mergePosition.x, duration, positionEase));
            sequence.Group(Tween.PositionY(sphereTransform, mergePosition.y, duration, positionEase));
            sequence.Group(Tween.Scale(sphereTransform, Vector3.one * targetScale, duration, scaleEase));
        }

        EventBus.Publish(new SpheresMergedEvent(group));
        sequence.ChainCallback(() => DestroyMergedSpheres(group));
    }

    private void DestroyMergedSpheres(GlassSphere2D[] group)
    {
        for (int i = 0; i < group.Length; i++)
        {
            GlassSphere2D sphere = group[i];
            if (sphere == null)
            {
                continue;
            }

            Destroy(sphere.gameObject);
        }
    }

    private bool CanUseContact(GlassSphere2D first, GlassSphere2D second)
    {
        return CanMerge(first)
            && CanMerge(second)
            && first != second
            && first.SphereColor == second.SphereColor;
    }

    private bool CanMerge(GlassSphere2D sphere)
    {
        return sphere != null
            && sphere.CurrentState == SphereState.Selected;
    }

    private bool ContainsContact(SphereContact contact)
    {
        for (int i = 0; i < contacts.Count; i++)
        {
            if (contacts[i].Matches(contact.First, contact.Second))
            {
                return true;
            }
        }

        return false;
    }

    private void RemoveContact(GlassSphere2D first, GlassSphere2D second)
    {
        for (int i = contacts.Count - 1; i >= 0; i--)
        {
            if (contacts[i].Matches(first, second))
            {
                contacts.RemoveAt(i);
            }
        }
    }

    private void RemoveContactsWith(GlassSphere2D sphere)
    {
        for (int i = contacts.Count - 1; i >= 0; i--)
        {
            if (contacts[i].Contains(sphere))
            {
                contacts.RemoveAt(i);
            }
        }
    }

    private static Vector3 GetAveragePosition(GlassSphere2D[] group)
    {
        Vector3 total = Vector3.zero;
        int count = 0;

        for (int i = 0; i < group.Length; i++)
        {
            GlassSphere2D sphere = group[i];
            if (sphere == null)
            {
                continue;
            }

            total += sphere.transform.position;
            count++;
        }

        return count > 0 ? total / count : Vector3.zero;
    }

    private readonly struct SphereContact
    {
        public readonly GlassSphere2D First;
        public readonly GlassSphere2D Second;

        public SphereContact(GlassSphere2D first, GlassSphere2D second)
        {
            First = first;
            Second = second;
        }

        public bool Matches(GlassSphere2D first, GlassSphere2D second)
        {
            return (First == first && Second == second)
                || (First == second && Second == first);
        }

        public bool Contains(GlassSphere2D sphere)
        {
            return First == sphere || Second == sphere;
        }

        public bool HasColor(SphereColors color)
        {
            return First != null
                && Second != null
                && First.SphereColor == color
                && Second.SphereColor == color;
        }

        public bool TouchesAny(List<GlassSphere2D> spheres)
        {
            return spheres.Contains(First) || spheres.Contains(Second);
        }
    }
}
