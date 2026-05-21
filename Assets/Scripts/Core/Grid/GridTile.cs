using Sirenix.OdinInspector;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    [SerializeField, ReadOnly] private GridCoordinates coordinates;
    [SerializeField] private bool isWalkable = true;
    [SerializeField] private Transform objectRoot;
    [SerializeField, ReadOnly] private GameObject currentPlacedObject;

    public GridCoordinates Coordinates => coordinates;
    public bool IsWalkable => isWalkable;
    public GameObject CurrentPlacedObject => currentPlacedObject;
    public Vector3 WorldPosition => transform.position;
    public Vector3 LocalPosition => transform.localPosition;

    public void Initialize(GridCoordinates coordinates)
    {
        this.coordinates = coordinates;
        gameObject.name = $"Tile_{coordinates.X}_{coordinates.Y}";
        EnsureObjectRoot();
    }

    public void SetWalkable(bool value)
    {
        isWalkable = value;
    }

    public void SetPlacedObject(GameObject placedObject)
    {
        EnsureObjectRoot();
        currentPlacedObject = placedObject;

        if (currentPlacedObject == null)
        {
            return;
        }

        currentPlacedObject.transform.SetParent(objectRoot, false);
        currentPlacedObject.transform.localPosition = Vector3.zero;
        currentPlacedObject.transform.localRotation = Quaternion.identity;
        currentPlacedObject.transform.localScale = Vector3.one;
    }

    public void ClearPlacedObject()
    {
        if (currentPlacedObject == null)
        {
            return;
        }

        GameObject objectToDestroy = currentPlacedObject;
        currentPlacedObject = null;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.Undo.DestroyObjectImmediate(objectToDestroy);
            return;
        }
#endif

        Destroy(objectToDestroy);
    }

    public bool HasPlacedObject()
    {
        return currentPlacedObject != null;
    }

    private void OnValidate()
    {
        EnsureObjectRoot();
    }

    private void EnsureObjectRoot()
    {
        if (objectRoot == null)
        {
            objectRoot = transform;
        }
    }
}
