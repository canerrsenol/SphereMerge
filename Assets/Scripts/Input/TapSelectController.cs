using Lean.Touch;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TapSelectController : MonoBehaviour
{
    [SerializeField] private Camera selectionCamera;
    [SerializeField] private LayerMask selectableLayerMask;
    [SerializeField] private bool ignoreGuiTouches = true;

    private Camera cachedCamera;

    private void Awake()
    {
        CacheCamera();
    }

    private void OnEnable()
    {
        CacheCamera();
        LeanTouch.OnFingerTap += HandleFingerTap;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerTap -= HandleFingerTap;
    }

    private void HandleFingerTap(LeanFinger finger)
    {
        if (ignoreGuiTouches && finger.StartedOverGui)
        {
            return;
        }

        Camera cameraToUse = selectionCamera != null ? selectionCamera : cachedCamera;
        if (cameraToUse == null)
        {
            Debug.LogWarning("TapSelectController needs a camera reference or a camera tagged MainCamera.", this);
            return;
        }

        Ray ray = cameraToUse.ScreenPointToRay(finger.ScreenPosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, selectableLayerMask);

        if (hit.collider == null)
        {
            return;
        }

        ISelectable selectable = GetSelectable(hit.collider);
        if (selectable == null)
        {
            return;
        }

        selectable.OnSelect();
    }

    private void CacheCamera()
    {
        cachedCamera = selectionCamera != null ? selectionCamera : Camera.main;
    }

    private static ISelectable GetSelectable(Collider2D collider)
    {
        if (collider.TryGetComponent(out ISelectable selectable))
        {
            return selectable;
        }

        selectable = collider.GetComponentInParent<ISelectable>();
        if (selectable != null)
        {
            return selectable;
        }

        Rigidbody2D attachedRigidbody = collider.attachedRigidbody;
        if (attachedRigidbody != null && attachedRigidbody.TryGetComponent(out selectable))
        {
            return selectable;
        }

        return null;
    }
}
