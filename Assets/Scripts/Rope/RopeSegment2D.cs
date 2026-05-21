using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(DistanceJoint2D))]
public class RopeSegment2D : MonoBehaviour
{
    public Rigidbody2D Rigidbody { get; private set; }

    private CircleCollider2D _collider;
    private DistanceJoint2D _joint;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        _joint = GetComponent<DistanceJoint2D>();

        foreach (HingeJoint2D hinge in GetComponents<HingeJoint2D>())
        {
            hinge.enabled = false;
        }

        if (_joint == null)
        {
            _joint = gameObject.AddComponent<DistanceJoint2D>();
        }
    }

    public void SetRadius(float radius)
    {
        _collider.radius = radius;
    }

    public void Connect(Rigidbody2D connectedBody, float distance)
    {
        _joint.autoConfigureConnectedAnchor = false;
        _joint.autoConfigureDistance = false;
        _joint.connectedBody = connectedBody;

        _joint.anchor = Vector2.zero;
        _joint.connectedAnchor = Vector2.zero;
        _joint.distance = distance;

        _joint.enableCollision = false;
    }
}
