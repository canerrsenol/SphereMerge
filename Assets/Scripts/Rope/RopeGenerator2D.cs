using System.Collections.Generic;
using UnityEngine;

public class RopeGenerator2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D leftAnchor;
    [SerializeField] private Rigidbody2D rightAnchor;
    [SerializeField] private RopeSegment2D segmentPrefab;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Rope Settings")]
    [SerializeField] private float sagAmount = 0.8f;
    [Min(0.001f)]
    [SerializeField] private float segmentRadius = 0.08f;

    [Header("Editor Preview")]
    [SerializeField] private bool showEditorPreview = true;
    [SerializeField] private Color previewLineColor = new(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private Color previewSegmentColor = new(1f, 0.85f, 0.2f, 1f);

    private readonly List<RopeSegment2D> _segments = new();

    private void Start()
    {
        ConfigureLineRenderer();
        GenerateRope();
    }

    private void LateUpdate()
    {
        UpdateLineRenderer();
    }

    private void GenerateRope()
    {
        ClearRope();

        Vector2 start = leftAnchor.position;
        Vector2 end = rightAnchor.position;

        int segmentCount = CalculateSegmentCount(start, end);

        Rigidbody2D previousBody = leftAnchor;
        Vector2 previousPosition = start;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (i + 1f) / (segmentCount + 1f);

            Vector2 position = CalculateRopePoint(start, end, t);

            RopeSegment2D segment = Instantiate(segmentPrefab, transform);
            segment.transform.localPosition = GetLocalRopePosition(position);
            segment.transform.localRotation = Quaternion.identity;
            segment.name = $"Rope Segment {i}";

            segment.SetRadius(segmentRadius);
            segment.Connect(previousBody, Vector2.Distance(previousPosition, position));

            _segments.Add(segment);
            previousBody = segment.Rigidbody;
            previousPosition = position;
        }

        // Son segmenti sag anchor'a bagla.
        RopeSegment2D lastSegment = _segments[^1];
        DistanceJoint2D endJoint = lastSegment.gameObject.AddComponent<DistanceJoint2D>();
        endJoint.autoConfigureConnectedAnchor = false;
        endJoint.autoConfigureDistance = false;
        endJoint.connectedBody = rightAnchor;
        endJoint.anchor = Vector2.zero;
        endJoint.connectedAnchor = Vector2.zero;
        endJoint.distance = Vector2.Distance(previousPosition, end);
        endJoint.enableCollision = false;
    }

    private void UpdateLineRenderer()
    {
        if (lineRenderer == null)
            return;

        ConfigureLineRenderer();
        lineRenderer.positionCount = _segments.Count + 2;

        lineRenderer.SetPosition(0, leftAnchor.position);

        for (int i = 0; i < _segments.Count; i++)
        {
            lineRenderer.SetPosition(i + 1, _segments[i].transform.position);
        }

        lineRenderer.SetPosition(_segments.Count + 1, rightAnchor.position);
    }

    private void ConfigureLineRenderer()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.useWorldSpace = true;
    }

    private Vector3 GetLocalRopePosition(Vector2 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        localPosition.z = 0f;
        return localPosition;
    }

    private int CalculateSegmentCount(Vector2 start, Vector2 end)
    {
        float desiredSpacing = segmentRadius * 2f;
        float anchorDistance = Vector2.Distance(start, end);
        return Mathf.Max(1, Mathf.CeilToInt(anchorDistance / desiredSpacing) - 1);
    }

    private Vector2 CalculateRopePoint(Vector2 start, Vector2 end, float t)
    {
        Vector2 position = Vector2.Lerp(start, end, t);
        float sag = Mathf.Sin(t * Mathf.PI) * sagAmount;
        position.y -= sag;
        return position;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showEditorPreview || leftAnchor == null || rightAnchor == null)
            return;

        Vector2 start = leftAnchor.position;
        Vector2 end = rightAnchor.position;
        int segmentCount = CalculateSegmentCount(start, end);

        Gizmos.color = previewLineColor;
        Vector2 previousPoint = start;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (i + 1f) / (segmentCount + 1f);
            Vector2 point = CalculateRopePoint(start, end, t);
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        Gizmos.DrawLine(previousPoint, end);

        Gizmos.color = previewSegmentColor;
        for (int i = 0; i < segmentCount; i++)
        {
            float t = (i + 1f) / (segmentCount + 1f);
            Vector2 point = CalculateRopePoint(start, end, t);
            Gizmos.DrawWireSphere(point, segmentRadius);
        }
    }

    private void ClearRope()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        _segments.Clear();
    }
}
