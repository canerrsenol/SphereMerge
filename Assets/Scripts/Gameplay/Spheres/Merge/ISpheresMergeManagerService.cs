// Receives sphere contact changes and starts valid merges.
public interface ISpheresMergeManagerService
{
    // Reports that two spheres are currently touching.
    void ReportSphereContact(GlassSphere2D first, GlassSphere2D second);
    // Reports that two spheres are no longer touching.
    void ReportSphereContactEnded(GlassSphere2D first, GlassSphere2D second);
}
