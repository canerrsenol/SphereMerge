using UnityEngine;

[DisallowMultipleComponent]
public sealed class SpriteLiquid2D : MonoBehaviour
{
    static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");
    static readonly int LiquidColorId = Shader.PropertyToID("_LiquidColor");
    static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
    static readonly int GlowIntensityId = Shader.PropertyToID("_GlowIntensity");
    static readonly int LiquidUpId = Shader.PropertyToID("_LiquidUp");
    static readonly int LiquidOffsetId = Shader.PropertyToID("_LiquidOffset");
    static readonly int SloshId = Shader.PropertyToID("_Slosh");
    static readonly int WaveAmountId = Shader.PropertyToID("_WaveAmount");
    static readonly int LiquidRadiusId = Shader.PropertyToID("_LiquidRadius");
    static readonly int EdgeSoftnessId = Shader.PropertyToID("_EdgeSoftness");

    [Header("Renderer")]
    [SerializeField] SpriteRenderer targetRenderer;

    [Header("Liquid")]
    [Range(0f, 1f)] [SerializeField] float fillAmount = 0.5f;
    [SerializeField] Color liquidColor = new Color(0.08f, 0.55f, 1f, 0.82f);
    [Range(0.1f, 0.7f)] [SerializeField] float liquidRadius = 0.47f;
    [Range(0.001f, 0.08f)] [SerializeField] float edgeSoftness = 0.015f;

    [Header("Glow")]
    [SerializeField] Color glowColor = new Color(0.25f, 0.9f, 1f, 1f);
    [Range(0f, 4f)] [SerializeField] float glowIntensity = 1f;

    [Header("Motion")]
    [SerializeField] Rigidbody2D targetRigidbody;
    [Range(0f, 0.18f)] [SerializeField] float waveAmount = 0.035f;
    [SerializeField] float velocityToSlosh = 0.025f;
    [SerializeField] float velocityToOffset = 0.012f;
    [SerializeField] float accelerationToSlosh = 0.035f;
    [SerializeField] float angularVelocityToSlosh = 0.0025f;
    [SerializeField] float collisionImpulseToSlosh = 0.018f;
    [SerializeField] float sloshSpring = 28f;
    [SerializeField] float sloshDamping = 7f;
    [SerializeField] float offsetSpring = 18f;
    [SerializeField] float offsetDamping = 5f;
    [SerializeField] float maxSlosh = 0.55f;
    [SerializeField] float maxLiquidOffset = 0.09f;

    MaterialPropertyBlock propertyBlock;
    Vector2 previousVelocity;
    Vector2 liquidOffset;
    Vector2 liquidOffsetVelocity;
    float slosh;
    float sloshVelocity;

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
        liquidColor = newLiquidColor;
        glowColor = newGlowColor;

        CacheRenderer();

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
        float target = (-localVelocity.x * velocityToSlosh) + (-localAcceleration.x * accelerationToSlosh) + (-targetRigidbody.angularVelocity * angularVelocityToSlosh);

        sloshVelocity += (target - slosh) * sloshSpring * fixedDeltaTime;
        sloshVelocity -= sloshVelocity * sloshDamping * fixedDeltaTime;
        slosh = Mathf.Clamp(slosh + sloshVelocity * fixedDeltaTime, -maxSlosh, maxSlosh);

        Vector2 targetOffset = Vector2.ClampMagnitude(-localVelocity * velocityToOffset, maxLiquidOffset);
        liquidOffsetVelocity += (targetOffset - liquidOffset) * offsetSpring * fixedDeltaTime;
        liquidOffsetVelocity -= liquidOffsetVelocity * offsetDamping * fixedDeltaTime;
        liquidOffset = Vector2.ClampMagnitude(liquidOffset + liquidOffsetVelocity * fixedDeltaTime, maxLiquidOffset);
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
        float impulse = collision.relativeVelocity.magnitude * collisionImpulseToSlosh;
        sloshVelocity += Mathf.Clamp(-localNormal.x * impulse, -maxSlosh, maxSlosh);
    }

    void OnValidate()
    {
        fillAmount = Mathf.Clamp01(fillAmount);
        liquidRadius = Mathf.Clamp(liquidRadius, 0.1f, 0.7f);
        edgeSoftness = Mathf.Clamp(edgeSoftness, 0.001f, 0.08f);
        waveAmount = Mathf.Clamp(waveAmount, 0f, 0.18f);
        maxSlosh = Mathf.Max(0f, maxSlosh);
        maxLiquidOffset = Mathf.Max(0f, maxLiquidOffset);

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
        Vector2 liquidUp = (worldUpInLocal + new Vector2(slosh, 0f)).normalized;

        propertyBlock.SetFloat(FillAmountId, fillAmount);
        propertyBlock.SetColor(LiquidColorId, liquidColor);
        propertyBlock.SetFloat(LiquidRadiusId, liquidRadius);
        propertyBlock.SetFloat(EdgeSoftnessId, edgeSoftness);
        propertyBlock.SetColor(GlowColorId, glowColor);
        propertyBlock.SetFloat(GlowIntensityId, glowIntensity);
        propertyBlock.SetFloat(WaveAmountId, waveAmount);
        propertyBlock.SetVector(LiquidUpId, new Vector4(liquidUp.x, liquidUp.y, 0f, 0f));
        propertyBlock.SetVector(LiquidOffsetId, new Vector4(liquidOffset.x, liquidOffset.y, 0f, 0f));
        propertyBlock.SetFloat(SloshId, slosh);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }
}
