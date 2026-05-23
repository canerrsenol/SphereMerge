using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class GlassSphere2D : MonoBehaviour, ISelectable
{
    private const float SelectedGravityScale = 1f;

    [SerializeField] private bool canSelect = true;
    [SerializeField] private SphereColors sphereColor;
    [SerializeField] private SphereColorsSO colorPalette;
    [SerializeField] private SpriteLiquid2D spriteLiquid;

    private Rigidbody2D _rigidbody2D;

    public bool CanSelect => canSelect;
    public SphereColors SphereColor => sphereColor;
    public SpriteLiquid2D SpriteLiquid => spriteLiquid;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        CacheReferences();
        ApplySphereColor();
    }

    private void Reset()
    {
        CacheReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
        ApplySphereColor();
    }

    public void OnSelect()
    {
        if (!canSelect)
        {
            return;
        }

        canSelect = false;
        _rigidbody2D.gravityScale = SelectedGravityScale;
    }

    public void SetSphereColor(SphereColors color)
    {
        sphereColor = color;
        CacheReferences();
        ApplySphereColor();
    }

    public void SetColorPalette(SphereColorsSO palette)
    {
        colorPalette = palette;
        CacheReferences();
        ApplySphereColor();
    }

    private void CacheReferences()
    {
        if (spriteLiquid == null)
        {
            spriteLiquid = GetComponentInChildren<SpriteLiquid2D>();
        }
    }

    private void ApplySphereColor()
    {
        CacheReferences();

        if (spriteLiquid == null)
        {
            return;
        }

        Color liquidColor = SphereColorsSO.GetDefaultLiquidColor(sphereColor);
        Color glowColor = SphereColorsSO.GetDefaultGlowColor(sphereColor);

        if (colorPalette != null)
        {
            colorPalette.TryGetColors(sphereColor, out liquidColor, out glowColor);
        }

        spriteLiquid.SetLiquidColors(liquidColor, glowColor);
    }
}
