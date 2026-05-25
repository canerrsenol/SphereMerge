using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class RopeLoadSensor2D : MonoBehaviour
{
    private readonly Dictionary<GlassSphere2D, HashSet<Collider2D>> loadedSpheres =
        new Dictionary<GlassSphere2D, HashSet<Collider2D>>();
    private Collider2D sensorCollider;

    public int LoadCount => loadedSpheres.Count;

    public event Action<int> LoadChanged;

    private void Awake()
    {
        ConfigureCollider();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<SpheresMergedEvent>(HandleSpheresMerged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SpheresMergedEvent>(HandleSpheresMerged);
        loadedSpheres.Clear();
    }

    private void Reset()
    {
        ConfigureCollider();
    }

    private void OnValidate()
    {
        ConfigureCollider();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTrackSphere(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryTrackSphere(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GlassSphere2D sphere = other.GetComponentInParent<GlassSphere2D>();
        if (sphere == null
            || !loadedSpheres.TryGetValue(sphere, out HashSet<Collider2D> colliders)
            || !colliders.Remove(other)
            || colliders.Count > 0)
        {
            return;
        }

        loadedSpheres.Remove(sphere);
        NotifyLoadChanged();
    }

    private void TryTrackSphere(Collider2D other)
    {
        GlassSphere2D sphere = other.GetComponentInParent<GlassSphere2D>();
        if (!CanCountSphere(sphere))
        {
            return;
        }

        if (!loadedSpheres.TryGetValue(sphere, out HashSet<Collider2D> colliders))
        {
            colliders = new HashSet<Collider2D>();
            loadedSpheres.Add(sphere, colliders);
            colliders.Add(other);
            NotifyLoadChanged();
            return;
        }

        colliders.Add(other);
    }

    private void HandleSpheresMerged(SpheresMergedEvent mergeEvent)
    {
        IReadOnlyList<GlassSphere2D> mergedSpheres = mergeEvent.MergedSpheres;
        if (mergedSpheres == null)
        {
            return;
        }

        bool loadChanged = false;
        for (int i = 0; i < mergedSpheres.Count; i++)
        {
            loadChanged |= loadedSpheres.Remove(mergedSpheres[i]);
        }

        if (loadChanged)
        {
            NotifyLoadChanged();
        }
    }

    private static bool CanCountSphere(GlassSphere2D sphere)
    {
        return sphere != null && sphere.CurrentState == SphereState.Selected;
    }

    private void ConfigureCollider()
    {
        if (sensorCollider == null)
        {
            sensorCollider = GetComponent<Collider2D>();
        }

        if (sensorCollider != null)
        {
            sensorCollider.isTrigger = true;
        }
    }

    private void NotifyLoadChanged()
    {
        LoadChanged?.Invoke(loadedSpheres.Count);
    }
}
