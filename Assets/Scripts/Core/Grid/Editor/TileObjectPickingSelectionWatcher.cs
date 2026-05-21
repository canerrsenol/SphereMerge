using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TileObjectPickingSelectionWatcher
{
    private static bool isOpeningPicker;

    static TileObjectPickingSelectionWatcher()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        Selection.selectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged()
    {
        if (isOpeningPicker || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            return;
        }

        GridTile selectedTile = selectedObject.GetComponentInParent<GridTile>();
        if (selectedTile == null)
        {
            return;
        }

        GridManager gridManager = FindPickingGridManager(selectedTile);
        if (gridManager == null)
        {
            return;
        }

        isOpeningPicker = true;
        TileObjectPickerWindow.Open(
            gridManager.TileObjectsFolderPath,
            prefab =>
            {
                gridManager.PlaceObjectOnTile(selectedTile, prefab);
                isOpeningPicker = false;
            },
            () => isOpeningPicker = false);
    }

    private static GridManager FindPickingGridManager(GridTile selectedTile)
    {
        GridManager[] gridManagers = Resources.FindObjectsOfTypeAll<GridManager>();
        for (int i = 0; i < gridManagers.Length; i++)
        {
            GridManager gridManager = gridManagers[i];
            if (gridManager == null
                || !gridManager.TileObjectPickingMode
                || EditorUtility.IsPersistent(gridManager)
                || !gridManager.OwnsTile(selectedTile))
            {
                continue;
            }

            return gridManager;
        }

        return null;
    }
}
