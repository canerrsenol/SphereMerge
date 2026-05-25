using System;
using System.Collections.Generic;
using UnityEngine;

// Counts selected spheres resting on a rope sensor area.
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class RopeLoadSensor2D : MonoBehaviour
{
    private readonly Dictionary<GlassSphere2D, HashSet<Collider2D>> loadedSpheres =
        new Dictionary<GlassSphere2D, HashSet<Collider2D>>();
    private Collider2D sensorCollider;

    public int LoadCount => loadedSpheres.Count;

    public event Action<int> LoadChanged;

    // Sets this collider up as a trigger sensor.
    private void Awake()
    {
        ConfigureCollider();
    }

    // Starts listening for merged spheres that must be removed from load.
    private void OnEnable()
    {
        EventBus.Subscribe<SpheresMergedEvent>(HandleSpheresMerged);
    }

    // Stops event listening and clears tracked sphere load.
    private void OnDisable()
    {
        EventBus.Unsubscribe<SpheresMergedEvent>(HandleSpheresMerged);
        loadedSpheres.Clear();
    }

    // Configures the collider when this component is added.
    private void Reset()
    {
        ConfigureCollider();
    }

    // Keeps the collider configured in the inspector.
    private void OnValidate()
    {
        ConfigureCollider();
    }

    // Starts tracking a sphere that enters the sensor.
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTrackSphere(other);
    }

    // Tracks spheres that become selectable while staying in the sensor.
    private void OnTriggerStay2D(Collider2D other)
    {
        TryTrackSphere(other);
    }

    // Stops tracking a sphere after its last collider exits.
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

    // Adds a selected sphere and its collider to the current load.
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

    // Removes merged spheres from the current rope load.
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

    // Returns true when a sphere is currently adding load.
    private static bool CanCountSphere(GlassSphere2D sphere)
    {
        return sphere != null && sphere.CurrentState == SphereState.Selected;
    }

    // Finds and configures the trigger collider.
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

    // Reports the new number of spheres on the rope.
    private void NotifyLoadChanged()
    {
        LoadChanged?.Invoke(loadedSpheres.Count);
    }
}
