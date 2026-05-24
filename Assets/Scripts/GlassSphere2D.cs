using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class GlassSphere2D : MonoBehaviour, ISelectable
{
    [SerializeField] private SphereColors sphereColor;
    [SerializeField] private SphereColorsSO colorPalette;
    [SerializeField] private SpriteLiquid2D spriteLiquid;
    [SerializeField] private SphereContactSensor2D contactSensor;
    [SerializeField] private GlassSphereVisual2D sphereVisual;

    private Rigidbody2D _rigidbody2D;
    private bool canSelect;
    private bool outlineVisible;
    private const float SelectedGravityScale = 1f;

    public bool CanSelect => canSelect;
    public SphereColors SphereColor => sphereColor;
    public SpriteLiquid2D SpriteLiquid => spriteLiquid;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        CacheReferences();
        if (contactSensor != null)
        {
            contactSensor.enabled = false;
        }

        SetOutlineVisible(false, true);
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

    private void Update()
    {
        bool shouldShowOutline = !canSelect
            && contactSensor != null
            && contactSensor.enabled
            && contactSensor.hasContact;

        SetOutlineVisible(shouldShowOutline);
    }

    public void OnSelect()
    {
        if (!canSelect)
        {
            sphereVisual?.PlayCannotSelectAnimation();
            return;
        }

        // Check if any click obstacles prevent selection
        var clickObstacles = GetComponentsInChildren<IClickManipulatorObstacle>();
        foreach (var obstacle in clickObstacles)
        {
            if (!obstacle.CanClick)
            {
                sphereVisual?.PlayCannotSelectAnimation();
                return;
            }
        }

        canSelect = false;
        _rigidbody2D.gravityScale = SelectedGravityScale;
        if (contactSensor != null)
        {
            contactSensor.enabled = true;
        }
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

        if (contactSensor == null)
        {
            contactSensor = GetComponentInChildren<SphereContactSensor2D>(true);
        }

        if (sphereVisual == null)
        {
            sphereVisual = GetComponentInChildren<GlassSphereVisual2D>(true);
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
        Color outlineColor = SphereColorsSO.GetDefaultOutlineColor(sphereColor);

        if (colorPalette != null)
        {
            colorPalette.TryGetColors(sphereColor, out liquidColor, out glowColor, out outlineColor);
        }

        spriteLiquid.SetLiquidColors(liquidColor, glowColor, outlineColor);
    }

    private void SetOutlineVisible(bool visible, bool force = false)
    {
        if (!force && outlineVisible == visible)
        {
            return;
        }

        outlineVisible = visible;

        if (spriteLiquid != null)
        {
            spriteLiquid.SetOutlineEnabled(visible);
        }
    }
}
