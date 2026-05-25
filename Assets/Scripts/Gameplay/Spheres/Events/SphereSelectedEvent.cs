public readonly struct SphereSelectedEvent
{
    public GlassSphere2D SelectedSphere { get; }

    public SphereSelectedEvent(GlassSphere2D selectedSphere)
    {
        SelectedSphere = selectedSphere;
    }
}