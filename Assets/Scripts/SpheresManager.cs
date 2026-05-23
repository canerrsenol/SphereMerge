using System;
using System.Reflection;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class SpheresManager : SerializedMonoBehaviour, ISpheresManagerService
{
    private const float DefaultIntroDuration = 0.25f;
    private const float DefaultIntroStagger = 0.03f;
    private const Ease DefaultIntroEase = Ease.OutBack;

    [SerializeField] private GlassSphere2D spherePrefab;
    [SerializeField] private SphereColorsSO sphereColors;
    [SerializeField, HideInInspector] private Vector2Int gridSize;
    [FormerlySerializedAs("spacing")]
    [SerializeField, HideInInspector] private Vector2 tileOffset = Vector2.one;
    [SerializeField] private SphereIntroAnimationSettingsSO introAnimationSettings;
    [SerializeField, HideInInspector] private Vector2Int storedGridSize;
    [OdinSerialize, HideInInspector] private GlassSphere2D[,] spheres = new GlassSphere2D[0, 0];

    private Sequence introSequence;

    public GlassSphere2D SpherePrefab => spherePrefab;
    public SphereColorsSO SphereColors => sphereColors;
    public Vector2Int GridSize => gridSize;
    public Vector2 TileOffset => tileOffset;
    public bool IsGridSizeValid => gridSize.x > 0 && gridSize.y > 0;

    private void Start()
    {
        ApplySphereColorPalettes();
        PlayIntroAnimation();
    }

    private void OnDestroy()
    {
        if (introSequence.isAlive)
        {
            introSequence.Stop();
        }

        introSequence = default;
    }

    private void OnValidate()
    {
        NormalizeGridSettings();
        EnsureGridDataSize();
        ApplySpherePositions();
        ApplySphereColorPalettes();
    }

    public void SetGridSettings(Vector2Int newGridSize, Vector2 newTileOffset, bool alignTransform, float topLocalY)
    {
        gridSize = newGridSize;
        tileOffset = newTileOffset;

        NormalizeGridSettings();
        EnsureGridDataSize();
        ApplySpherePositions();

        if (alignTransform)
        {
            AlignTransformToGrid(topLocalY);
        }
    }

    public float GetGridTopLocalY()
    {
        if (!IsGridSizeValid)
        {
            return transform.localPosition.y;
        }

        return transform.localPosition.y + GetGridEdgeOffset(gridSize.y, Mathf.Abs(tileOffset.y));
    }

    private void AlignTransformToGrid(float topLocalY)
    {
        if (!IsGridSizeValid)
        {
            return;
        }

        Vector3 localPosition = transform.localPosition;
        localPosition.x = -GetGridCenterOffset(gridSize.x, tileOffset.x);
        localPosition.y = topLocalY - GetGridEdgeOffset(gridSize.y, Mathf.Abs(tileOffset.y));
        transform.localPosition = localPosition;
    }

    public GlassSphere2D GetSphere(Vector2Int position)
    {
        if (!TryGetArrayPosition(position, out int x, out int y))
        {
            return null;
        }

        return spheres[x, y];
    }

    public void SetSphere(Vector2Int position, GlassSphere2D sphere)
    {
        if (!TryGetArrayPosition(position, out int x, out int y))
        {
            return;
        }

        spheres[x, y] = sphere;

        if (sphere != null && sphere.transform.parent != transform)
        {
            sphere.transform.SetParent(transform, true);
        }

        if (sphere != null)
        {
            sphere.SetColorPalette(sphereColors);
        }
    }

    public void SetSphereColors(SphereColorsSO newSphereColors)
    {
        sphereColors = newSphereColors;
        ApplySphereColorPalettes();
    }

    public void ClearGridData()
    {
        EnsureGridDataSize();

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                spheres[x, y] = null;
            }
        }
    }

    public Vector3 GetLocalPosition(Vector2Int position)
    {
        return new Vector3(position.x * tileOffset.x, position.y * Mathf.Abs(tileOffset.y), 0f);
    }

    public bool IsPositionValid(Vector2Int position)
    {
        return IsGridSizeValid
            && position.x >= 0
            && position.y >= 0
            && position.x < gridSize.x
            && position.y < gridSize.y;
    }

    public void EnsureGridDataSize()
    {
        if (!IsGridSizeValid)
        {
            spheres = new GlassSphere2D[0, 0];
            storedGridSize = gridSize;
            return;
        }

        if (storedGridSize == gridSize
            && spheres != null
            && spheres.GetLength(0) == gridSize.x
            && spheres.GetLength(1) == gridSize.y)
        {
            return;
        }

        ResizeSpheresPreservingPositions();
    }

    private void NormalizeGridSettings()
    {
        if (gridSize.x < 0 || gridSize.y < 0)
        {
            gridSize = new Vector2Int(Mathf.Max(0, gridSize.x), Mathf.Max(0, gridSize.y));
        }

        if (tileOffset.y < 0f)
        {
            tileOffset.y = -tileOffset.y;
        }
    }

    private static float GetGridCenterOffset(int cellCount, float tileOffset)
    {
        return GetGridEdgeOffset(cellCount, tileOffset) * 0.5f;
    }

    private static float GetGridEdgeOffset(int cellCount, float tileOffset)
    {
        return Mathf.Max(0, cellCount - 1) * tileOffset;
    }

    private bool TryGetArrayPosition(Vector2Int position, out int x, out int y)
    {
        EnsureGridDataSize();

        if (!IsPositionValid(position))
        {
            x = -1;
            y = -1;
            return false;
        }

        x = position.x;
        y = position.y;
        return true;
    }

    private void ResizeSpheresPreservingPositions()
    {
        GlassSphere2D[,] previousSpheres = spheres;
        Vector2Int previousGridSize = storedGridSize;

        spheres = new GlassSphere2D[gridSize.x, gridSize.y];

        if (previousSpheres != null && previousGridSize.x > 0 && previousGridSize.y > 0)
        {
            int copyWidth = Mathf.Min(previousGridSize.x, gridSize.x);
            int copyHeight = Mathf.Min(previousGridSize.y, gridSize.y);

            for (int x = 0; x < copyWidth; x++)
            {
                for (int y = 0; y < copyHeight; y++)
                {
                    if (x < previousSpheres.GetLength(0) && y < previousSpheres.GetLength(1))
                    {
                        spheres[x, y] = previousSpheres[x, y];
                    }
                }
            }
        }

        storedGridSize = gridSize;
    }

    private void ApplySpherePositions()
    {
        if (!IsGridSizeValid || spheres == null)
        {
            return;
        }

        int width = Mathf.Min(gridSize.x, spheres.GetLength(0));
        int height = Mathf.Min(gridSize.y, spheres.GetLength(1));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GlassSphere2D sphere = spheres[x, y];
                if (sphere == null)
                {
                    continue;
                }

                Transform sphereTransform = sphere.transform;
                if (sphereTransform.parent == transform)
                {
                    sphereTransform.localPosition = GetLocalPosition(new Vector2Int(x, y));
                }
            }
        }
    }

    private void ApplySphereColorPalettes()
    {
        if (!IsGridSizeValid || spheres == null)
        {
            return;
        }

        int width = Mathf.Min(gridSize.x, spheres.GetLength(0));
        int height = Mathf.Min(gridSize.y, spheres.GetLength(1));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GlassSphere2D sphere = spheres[x, y];
                if (sphere == null)
                {
                    continue;
                }

                sphere.SetColorPalette(sphereColors);
            }
        }
    }

    private void PlayIntroAnimation()
    {
        if (!IsGridSizeValid || spheres == null)
        {
            return;
        }

        introSequence = Sequence.Create();

        float duration = introAnimationSettings != null ? introAnimationSettings.Duration : DefaultIntroDuration;
        float stagger = introAnimationSettings != null ? introAnimationSettings.Stagger : DefaultIntroStagger;
        Ease ease = introAnimationSettings != null ? introAnimationSettings.Ease : DefaultIntroEase;

        int width = Mathf.Min(gridSize.x, spheres.GetLength(0));
        int height = Mathf.Min(gridSize.y, spheres.GetLength(1));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GlassSphere2D sphere = spheres[x, y];
                if (sphere == null)
                {
                    continue;
                }

                Transform sphereTransform = sphere.transform;
                Vector3 targetScale = sphereTransform.localScale;
                sphereTransform.localScale = Vector3.zero;

                introSequence.Group(Tween.Scale(
                    sphereTransform,
                    targetScale,
                    duration,
                    ease,
                    startDelay: (x + y) * stagger));
            }
        }
    }

    [Button("Edit Level")]
    private void OpenLevelEditor()
    {
#if UNITY_EDITOR
        Type editorWindowType = Type.GetType("SpheresManagerEditorWindow, Assembly-CSharp-Editor");
        MethodInfo openMethod = editorWindowType?.GetMethod("Open", BindingFlags.Public | BindingFlags.Static);
        openMethod?.Invoke(null, new object[] { this });
#endif
    }
}
