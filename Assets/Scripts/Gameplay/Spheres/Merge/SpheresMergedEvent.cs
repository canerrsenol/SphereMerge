using System.Collections.Generic;

// Carries spheres that have completed a merge operation.
public readonly struct SpheresMergedEvent
{
    public IReadOnlyList<GlassSphere2D> MergedSpheres { get; }

    // Creates an event with the merged sphere group.
    public SpheresMergedEvent(IReadOnlyList<GlassSphere2D> mergedSpheres)
    {
        MergedSpheres = mergedSpheres;
    }
}
