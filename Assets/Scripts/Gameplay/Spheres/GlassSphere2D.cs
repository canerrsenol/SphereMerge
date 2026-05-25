using UnityEngine;

public enum SphereState { Idle, IdleFirstInColumn, Selected, Merged }

[RequireComponent(typeof(Rigidbody2D))]
public sealed class GlassSphere2D : MonoBehaviour, ISelectable
{
    [SerializeField] private SphereColors sphereColor;
    [SerializeField] private SphereColorsSO colorPalette;
    [SerializeField] private SpriteLiquid2D spriteLiquid;
    [SerializeField] private SphereContactSensor2D contactSensor;
    [SerializeField] private GlassSphereVisual2D sphereVisual;

    private Rigidbody2D _rigidbody2D;
    private SphereState currentState = SphereState.Idle;
    private bool canSelect = false;
    private Collider2D[] colliders;
    private bool[] initialColliderEnabledStates;
    private bool outlineVisible;

    private const float IdleGravityScale = 0f;
    private const float SelectedGravityScale = 1f;

    public bool CanSelect => canSelect;
    public SphereState CurrentState => currentState;
    public SphereColors SphereColor => sphereColor;
    public SpriteLiquid2D SpriteLiquid => spriteLiquid;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        CacheReferences();
        CacheColliders(true);
        ApplyState(currentState, true);
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
        bool shouldShowOutline = currentState == SphereState.Selected
            && contactSensor != null
            && contactSensor.enabled
            && contactSensor.hasContact;

        SetOutlineVisible(shouldShowOutline);
    }

    public void OnSelect()
    {
        // Check if any click obstacles prevent selection
        var clickObstacles = GetComponentsInChildren<IClickManipulatorObstacle>();
        foreach (var obstacle in clickObstacles)
        {
            if (!obstacle.CanClick)
            {
                return;
            }
        }

        if (!canSelect || currentState != SphereState.IdleFirstInColumn)
        {
            if (currentState == SphereState.Idle)
            {
                sphereVisual?.PlayCannotSelectAnimation();
            }

            return;
        }

        SetSphereState(SphereState.Selected);
        EventBus.Publish(new SphereSelectedEvent(this));
    }

    public void SetSphereState(SphereState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
        ApplyState(newState);
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

        CacheColliders();
    }

    private void CacheColliders(bool force = false)
    {
        Collider2D[] foundColliders = GetComponentsInChildren<Collider2D>(true);
        if (!force && colliders != null && colliders.Length == foundColliders.Length)
        {
            return;
        }

        colliders = foundColliders;
        initialColliderEnabledStates = new bool[colliders.Length];

        for (int i = 0; i < colliders.Length; i++)
        {
            initialColliderEnabledStates[i] = colliders[i] != null && colliders[i].enabled;
        }
    }

    private void ApplyState(SphereState state, bool forceOutline = false)
    {
        CacheReferences();

        switch (state)
        {
            case SphereState.Idle:
                canSelect = false;
                SetRigidbodyActive(true);
                SetCollidersEnabled(true);
                SetGravityScale(IdleGravityScale);
                SetContactSensorEnabled(false);
                SetOutlineVisible(false, forceOutline);
                break;

            case SphereState.IdleFirstInColumn:
                canSelect = true;
                SetRigidbodyActive(true);
                SetCollidersEnabled(true);
                SetGravityScale(IdleGravityScale);
                SetContactSensorEnabled(false);
                SetOutlineVisible(false, forceOutline);
                break;

            case SphereState.Selected:
                canSelect = false;
                SetRigidbodyActive(true);
                SetCollidersEnabled(true);
                SetGravityScale(SelectedGravityScale);
                SetContactSensorEnabled(true);
                break;

            case SphereState.Merged:
                canSelect = false;
                SetContactSensorEnabled(false);
                SetOutlineVisible(true, forceOutline);
                SetRigidbodyActive(false);
                SetCollidersEnabled(false);
                break;
        }
    }

    private void SetGravityScale(float gravityScale)
    {
        if (_rigidbody2D != null)
        {
            _rigidbody2D.gravityScale = gravityScale;
        }
    }

    private void SetRigidbodyActive(bool active)
    {
        if (_rigidbody2D == null)
        {
            return;
        }

        if (!active)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
        }

        _rigidbody2D.simulated = active;
    }

    private void SetContactSensorEnabled(bool enabled)
    {
        if (contactSensor != null)
        {
            contactSensor.enabled = enabled;
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        CacheColliders();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
            {
                continue;
            }

            colliders[i].enabled = enabled && initialColliderEnabledStates[i];
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
