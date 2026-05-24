using UnityEngine;
using VContainer;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class SphereContactSensor2D : MonoBehaviour
{
    [SerializeField] private GlassSphere2D sphere;

    private ISpheresMergeManagerService mergeManagerService;

    public GlassSphere2D Sphere => sphere;

    [Inject]
    public void Construct(ISpheresMergeManagerService mergeManagerService)
    {
        this.mergeManagerService = mergeManagerService;
    }

    private void Awake()
    {
        CacheReferences();
    }

    private void Reset()
    {
        CacheReferences();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReportContact(collision, true);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ReportContact(collision, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        ReportContact(collision, false);
    }

    private void ReportContact(Collision2D collision, bool isTouching)
    {
        CacheReferences();

        if (sphere == null)
        {
            return;
        }

        GlassSphere2D otherSphere = collision.collider.GetComponentInParent<GlassSphere2D>();
        if (otherSphere == null || otherSphere == sphere)
        {
            return;
        }

        if (mergeManagerService == null)
        {
            return;
        }

        if (isTouching)
        {
            mergeManagerService.ReportSphereContact(sphere, otherSphere);
        }
        else
        {
            mergeManagerService.ReportSphereContactEnded(sphere, otherSphere);
        }
    }

    private void CacheReferences()
    {
        if (sphere == null)
        {
            sphere = GetComponentInParent<GlassSphere2D>();
        }
    }
}
