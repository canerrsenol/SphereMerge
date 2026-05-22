using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class GlassSphere2D : MonoBehaviour, ISelectable
{
    private const float SelectedGravityScale = 1f;

    [SerializeField] private bool canSelect = true;
    [SerializeField] private SphereColors sphereColor;

    private Rigidbody2D _rigidbody2D;

    public bool CanSelect => canSelect;
    public SphereColors SphereColor => sphereColor;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
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
    }
}
