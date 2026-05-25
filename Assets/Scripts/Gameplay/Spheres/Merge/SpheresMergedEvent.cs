using System.Collections.Generic;

public readonly struct SpheresMergedEvent
{
    public IReadOnlyList<GlassSphere2D> MergedSpheres { get; }

    public SpheresMergedEvent(IReadOnlyList<GlassSphere2D> mergedSpheres)
    {
        MergedSpheres = mergedSpheres;
    }
}
