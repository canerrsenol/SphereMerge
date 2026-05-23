using UnityEngine;

[CreateAssetMenu(fileName = "SpriteLiquidMotionSettings", menuName = "Sphere Merge/Sprite Liquid Motion Settings")]
public sealed class SpriteLiquidMotionSettingsSO : ScriptableObject
{
    [Header("Tilt")]
    [SerializeField, Range(0f, 0.08f)] private float speedToTilt = 0.025f;
    [SerializeField, Range(0f, 0.08f)] private float accelerationToTilt = 0.035f;
    [SerializeField, Range(0f, 0.006f)] private float angularVelocityToTilt = 0.0025f;
    [SerializeField, Range(0f, 0.04f)] private float collisionToTilt = 0.018f;

    [Header("Feel")]
    [SerializeField, Min(0f)] private float spring = 28f;
    [SerializeField, Min(0f)] private float damping = 7f;
    [SerializeField, Range(0f, 1f)] private float maxTilt = 0.65f;

    [Header("Lag")]
    [SerializeField, Range(0f, 0.4f)] private float speedToOffset = 0.12f;
    [SerializeField, Range(0f, 0.12f)] private float maxOffset = 0.07f;

    public float SpeedToTilt => speedToTilt;
    public float AccelerationToTilt => accelerationToTilt;
    public float AngularVelocityToTilt => angularVelocityToTilt;
    public float CollisionToTilt => collisionToTilt;
    public float Spring => spring;
    public float Damping => damping;
    public float MaxTilt => maxTilt;
    public float SpeedToOffset => speedToOffset;
    public float MaxOffset => maxOffset;

    private void OnValidate()
    {
        speedToTilt = Mathf.Max(0f, speedToTilt);
        accelerationToTilt = Mathf.Max(0f, accelerationToTilt);
        angularVelocityToTilt = Mathf.Max(0f, angularVelocityToTilt);
        collisionToTilt = Mathf.Max(0f, collisionToTilt);
        spring = Mathf.Max(0f, spring);
        damping = Mathf.Max(0f, damping);
        maxTilt = Mathf.Max(0f, maxTilt);
        speedToOffset = Mathf.Max(0f, speedToOffset);
        maxOffset = Mathf.Max(0f, maxOffset);
    }
}
