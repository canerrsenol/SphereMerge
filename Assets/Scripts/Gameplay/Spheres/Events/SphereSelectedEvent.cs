// Carries the sphere selected by player input.
public readonly struct SphereSelectedEvent
{
    public GlassSphere2D SelectedSphere { get; }

    // Creates an event for a selected sphere.
    public SphereSelectedEvent(GlassSphere2D selectedSphere)
    {
        SelectedSphere = selectedSphere;
    }
}
