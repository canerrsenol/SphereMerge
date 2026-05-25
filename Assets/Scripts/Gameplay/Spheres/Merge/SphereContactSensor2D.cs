using System.Collections.Generic;
using UnityEngine;
using VContainer;

// Reports contact between selected spheres to the merge system.
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class SphereContactSensor2D : MonoBehaviour
{
    [SerializeField] private GlassSphere2D sphere;

    private ISpheresMergeManagerService mergeManagerService;
    private readonly HashSet<GlassSphere2D> sameColorContacts = new HashSet<GlassSphere2D>();
    private bool hasContactValue;

    public GlassSphere2D Sphere => sphere;
    public bool hasContact
    {
        get
        {
            RefreshContactState();
            return hasContactValue;
        }
    }

    public bool HasContact => hasContact;

    [Inject]
    // Receives the service that checks contacts for merges.
    public void Construct(ISpheresMergeManagerService mergeManagerService)
    {
        this.mergeManagerService = mergeManagerService;
    }

    // Finds the sphere owned by this sensor.
    private void Awake()
    {
        CacheReferences();
    }

    // Refreshes the sphere reference when the sensor is added.
    private void Reset()
    {
        CacheReferences();
    }

    // Reports the start of physical contact.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReportContact(collision, true);
    }

    // Keeps reporting contact while spheres continue touching.
    private void OnCollisionStay2D(Collision2D collision)
    {
        ReportContact(collision, true);
    }

    // Reports the end of physical contact.
    private void OnCollisionExit2D(Collision2D collision)
    {
        ReportContact(collision, false);
    }

    // Clears visible match contact when this sensor is disabled.
    private void OnDisable()
    {
        sameColorContacts.Clear();
        hasContactValue = false;
    }

    // Sends a valid sphere contact change to the merge manager.
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

        UpdateContactState(otherSphere, isTouching);

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

    // Tracks matching contacts used by the sphere outline.
    private void UpdateContactState(GlassSphere2D otherSphere, bool isTouching)
    {
        if (isTouching && sphere.SphereColor == otherSphere.SphereColor)
        {
            sameColorContacts.Add(otherSphere);
        }
        else
        {
            sameColorContacts.Remove(otherSphere);
        }

        RefreshContactState();
    }

    // Removes missing contacts and updates the contact flag.
    private void RefreshContactState()
    {
        sameColorContacts.RemoveWhere(contact => contact == null);
        hasContactValue = sameColorContacts.Count > 0;
    }

    // Finds the parent sphere when it was not assigned.
    private void CacheReferences()
    {
        if (sphere == null)
        {
            sphere = GetComponentInParent<GlassSphere2D>();
        }
    }
}
