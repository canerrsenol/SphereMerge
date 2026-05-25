using UnityEngine;

// Enables the next selectable sphere in each grid column.
[DisallowMultipleComponent]
[RequireComponent(typeof(SpheresManager))]
public sealed class SphereColumnActivationController : MonoBehaviour
{
    [SerializeField] private SpheresManager spheresManager;

    // Finds the grid component used for sphere activation.
    private void Awake()
    {
        CacheReferences();
    }

    // Starts listening for sphere selections.
    private void OnEnable()
    {
        CacheReferences();
        EventBus.Subscribe<SphereSelectedEvent>(HandleSphereSelected);
    }

    // Stops listening for sphere selections.
    private void OnDisable()
    {
        EventBus.Unsubscribe<SphereSelectedEvent>(HandleSphereSelected);
    }

    // Refreshes the grid reference when this component is added.
    private void Reset()
    {
        CacheReferences();
    }

    // Makes the lowest available sphere in each column selectable.
    public void ActivateInitialSpheres()
    {
        if (spheresManager == null || !spheresManager.IsGridSizeValid)
        {
            return;
        }

        Vector2Int gridSize = spheresManager.GridSize;
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                GlassSphere2D sphere = spheresManager.GetSphere(new Vector2Int(x, y));
                if (sphere == null)
                {
                    continue;
                }

                if (sphere.CurrentState == SphereState.Idle)
                {
                    sphere.SetSphereState(SphereState.IdleFirstInColumn);
                }

                break;
            }
        }
    }

    // Enables the sphere above the sphere selected by the player.
    private void HandleSphereSelected(SphereSelectedEvent selectionEvent)
    {
        GlassSphere2D sphere = selectionEvent.SelectedSphere;
        if (spheresManager == null
            || sphere == null
            || !spheresManager.TryFindSpherePosition(sphere, out Vector2Int position))
        {
            return;
        }

        ActivateSphereAbove(position);
    }

    // Makes one sphere above a selected cell selectable.
    private void ActivateSphereAbove(Vector2Int position)
    {
        GlassSphere2D aboveSphere = spheresManager.GetSphere(new Vector2Int(position.x, position.y + 1));
        if (aboveSphere != null && aboveSphere.CurrentState == SphereState.Idle)
        {
            aboveSphere.SetSphereState(SphereState.IdleFirstInColumn);
        }
    }

    // Finds the sphere grid on this object when it is not assigned.
    private void CacheReferences()
    {
        if (spheresManager == null)
        {
            spheresManager = GetComponent<SpheresManager>();
        }
    }
}
