using UnityEngine;

[DisallowMultipleComponent]
public sealed class SpriteLiquid2D : MonoBehaviour
{
    const float DefaultSpeedToTilt = 0.025f;
    const float DefaultAccelerationToTilt = 0.035f;
    const float DefaultAngularVelocityToTilt = 0.0025f;
    const float DefaultCollisionToTilt = 0.018f;
    const float DefaultSpring = 28f;
    const float DefaultDamping = 7f;
    const float DefaultMaxTilt = 0.65f;
    const float DefaultSpeedToOffset = 0.12f;
    const float DefaultMaxOffset = 0.07f;

    static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");
    static readonly int LiquidColorId = Shader.PropertyToID("_LiquidColor");
    static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
    static readonly int GlowIntensityId = Shader.PropertyToID("_GlowIntensity");
    static readonly int OutlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
    static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int LiquidUpId = Shader.PropertyToID("_LiquidUp");
    static readonly int LiquidOffsetId = Shader.PropertyToID("_LiquidOffset");

    [Header("References")]
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] Rigidbody2D targetRigidbody;
    [SerializeField] SpriteLiquidMotionSettingsSO motionSettings;

    [Header("Liquid")]
    [Range(0f, 1f)] [SerializeField] float fillAmount = 0.5f;
    [SerializeField] Color liquidColor = new Color(0.08f, 0.55f, 1f, 0.82f);
    [SerializeField] Color glowColor = new Color(0.25f, 0.9f, 1f, 1f);
    [Range(0f, 4f)] [SerializeField] float glowIntensity = 1f;
    [SerializeField] bool outlineEnabled = true;
    [Range(0f, 16f)] [SerializeField] float outlineWidth = 4f;
    [SerializeField] Color outlineColor = new Color(0.03f, 0.22f, 0.55f, 1f);

    MaterialPropertyBlock propertyBlock;
    Vector2 previousVelocity;
    Vector2 liquidOffset;
    Vector2 liquidOffsetVelocity;
    float tilt;
    float tiltVelocity;

    public float FillAmount
    {
        get => fillAmount;
        set
        {
            fillAmount = Mathf.Clamp01(value);
            ApplyProperties();
        }
    }

    public void SetLiquidColors(Color newLiquidColor, Color newGlowColor)
    {
        SetLiquidColors(newLiquidColor, newGlowColor, outlineColor);
    }

    public void SetLiquidColors(Color newLiquidColor, Color newGlowColor, Color newOutlineColor)
    {
        liquidColor = newLiquidColor;
        glowColor = newGlowColor;
        outlineColor = newOutlineColor;

        CacheRenderer();

        ApplyProperties();
    }

    public void SetOutlineEnabled(bool enabled)
    {
        outlineEnabled = enabled;
        ApplyProperties();
    }

    public void SetOutlineWidth(float width)
    {
        outlineWidth = Mathf.Max(0f, width);
        ApplyProperties();
    }

    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        ApplyProperties();
    }

    void Awake()
    {
        CacheRenderer();
        propertyBlock = new MaterialPropertyBlock();

        if (targetRigidbody == null)
            targetRigidbody = GetComponent<Rigidbody2D>();

        if (targetRigidbody != null)
            previousVelocity = targetRigidbody.linearVelocity;

        ApplyProperties();
    }

    void Reset()
    {
        targetRigidbody = GetComponent<Rigidbody2D>();
        CacheRenderer();
    }

    void FixedUpdate()
    {
        if (targetRigidbody == null)
            return;

        float fixedDeltaTime = Time.fixedDeltaTime;
        Vector2 velocity = targetRigidbody.linearVelocity;
        Vector2 acceleration = fixedDeltaTime > 0f ? (velocity - previousVelocity) / fixedDeltaTime : Vector2.zero;
        previousVelocity = velocity;

        Vector2 localVelocity = transform.InverseTransformDirection(velocity);
        Vector2 localAcceleration = transform.InverseTransformDirection(acceleration);
        float targetTilt =
            (-localVelocity.x * SpeedToTilt) +
            (-localAcceleration.x * AccelerationToTilt) +
            (-targetRigidbody.angularVelocity * AngularVelocityToTilt);

        float damping = Mathf.Exp(-Damping * fixedDeltaTime);
        tiltVelocity += (targetTilt - tilt) * Spring * fixedDeltaTime;
        tiltVelocity *= damping;
        tilt = Mathf.Clamp(tilt + tiltVelocity * fixedDeltaTime, -MaxTilt, MaxTilt);

        Vector2 targetOffset = Vector2.ClampMagnitude(-localVelocity * SpeedToOffset, MaxOffset);
        liquidOffsetVelocity += (targetOffset - liquidOffset) * Spring * fixedDeltaTime;
        liquidOffsetVelocity *= damping;
        liquidOffset = Vector2.ClampMagnitude(liquidOffset + liquidOffsetVelocity * fixedDeltaTime, MaxOffset);
    }

    void LateUpdate()
    {
        ApplyProperties();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount == 0)
            return;

        Vector2 localNormal = transform.InverseTransformDirection(collision.GetContact(0).normal);
        float impulse = collision.relativeVelocity.magnitude * CollisionToTilt;
        tiltVelocity += Mathf.Clamp(-localNormal.x * impulse, -MaxTilt, MaxTilt);
    }

    void OnValidate()
    {
        fillAmount = Mathf.Clamp01(fillAmount);
        glowIntensity = Mathf.Max(0f, glowIntensity);
        outlineWidth = Mathf.Max(0f, outlineWidth);

        if (Application.isPlaying == false)
        {
            CacheRenderer();
            propertyBlock ??= new MaterialPropertyBlock();
            ApplyProperties();
        }
    }

    void CacheRenderer()
    {
        if (targetRenderer != null)
            return;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (renderer != null && renderer.transform != transform && renderer.enabled)
            {
                targetRenderer = renderer;
                return;
            }
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (renderer != null && renderer.transform != transform)
            {
                targetRenderer = renderer;
                return;
            }
        }

        targetRenderer = GetComponent<SpriteRenderer>();
    }

    void ApplyProperties()
    {
        CacheRenderer();

        if (targetRenderer == null)
            return;

        propertyBlock ??= new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(propertyBlock);

        Vector2 worldUpInLocal = transform.InverseTransformDirection(Vector2.up);
        Vector2 liquidUp = (worldUpInLocal + new Vector2(tilt, 0f)).normalized;

        propertyBlock.SetFloat(FillAmountId, fillAmount);
        propertyBlock.SetColor(LiquidColorId, liquidColor);
        propertyBlock.SetColor(GlowColorId, glowColor);
        propertyBlock.SetFloat(GlowIntensityId, glowIntensity);
        propertyBlock.SetFloat(OutlineEnabledId, outlineEnabled ? 1f : 0f);
        propertyBlock.SetFloat(OutlineWidthId, outlineWidth);
        propertyBlock.SetColor(OutlineColorId, outlineColor);
        propertyBlock.SetVector(LiquidUpId, new Vector4(liquidUp.x, liquidUp.y, 0f, 0f));
        propertyBlock.SetVector(LiquidOffsetId, new Vector4(liquidOffset.x, liquidOffset.y, 0f, 0f));
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    float SpeedToTilt => motionSettings != null ? motionSettings.SpeedToTilt : DefaultSpeedToTilt;
    float AccelerationToTilt => motionSettings != null ? motionSettings.AccelerationToTilt : DefaultAccelerationToTilt;
    float AngularVelocityToTilt => motionSettings != null ? motionSettings.AngularVelocityToTilt : DefaultAngularVelocityToTilt;
    float CollisionToTilt => motionSettings != null ? motionSettings.CollisionToTilt : DefaultCollisionToTilt;
    float Spring => motionSettings != null ? motionSettings.Spring : DefaultSpring;
    float Damping => motionSettings != null ? motionSettings.Damping : DefaultDamping;
    float MaxTilt => motionSettings != null ? motionSettings.MaxTilt : DefaultMaxTilt;
    float SpeedToOffset => motionSettings != null ? motionSettings.SpeedToOffset : DefaultSpeedToOffset;
    float MaxOffset => motionSettings != null ? motionSettings.MaxOffset : DefaultMaxOffset;
}
