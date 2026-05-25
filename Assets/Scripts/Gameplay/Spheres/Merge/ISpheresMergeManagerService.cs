public interface ISpheresMergeManagerService
{
    void ReportSphereContact(GlassSphere2D first, GlassSphere2D second);
    void ReportSphereContactEnded(GlassSphere2D first, GlassSphere2D second);
}
