using UnityEngine;

public class WormPart : MonoBehaviour
{
    public bool IsGrounded { get; private set; }
    public GameObject GroundObject { get; private set; }
    public Vector3 GroundNormal { get; private set; }
    public float TimeSinceLastGrounded { get; private set; }

    private Collider[] _colliders;
    private readonly Collider[] _results = new Collider[GameParameters.GroundColliderMaxHeldCollisions];
    private readonly float _verticalDetectionOffset = GameParameters.GroundingColliderVerticalDetectionOffset;
    private readonly float _detectionRadiusScale = GameParameters.GroundColliderDetectionRadiusScale;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>();
    }

    protected virtual void FixedUpdate()
    {
        CheckGrounded();
    }

    public virtual void MoveForward()
    {
        
    }

    public virtual void Jump()
    {
        
    }

    public virtual void Headbut()
    {
        
    }

    private void CheckGrounded()
{
    Bounds bounds = GetCombinedBounds();
    IsGrounded = false;
    GroundObject = null;
    GroundNormal = Vector3.up;

    for (int i = 0; i < _colliders.Length; i++)
    {
        Collider col = _colliders[i];
        
        float uprightDot = Vector3.Dot(col.transform.up, Vector3.up);
        if (uprightDot < 0.5f) continue;

        Vector3 bottom = GetColliderBottom(col);
        Vector3 checkPos = bottom + Vector3.down * _verticalDetectionOffset;
        float radius = GetColliderRadius(col);

        int hitCount = Physics.OverlapSphereNonAlloc(checkPos, radius, _results, ~0, QueryTriggerInteraction.Ignore);

        for (int j = 0; j < hitCount; j++)
        {
            Collider hit = _results[j];
            if (IsOurCollider(hit)) continue;
            if (hit.CompareTag("CreaturePart")) continue;

            IsGrounded = true;
            GroundObject = hit.gameObject;
            GroundNormal = GetGroundNormal(hit);
            break;
        }

        if (IsGrounded) break;
    }
}

private Vector3 GetColliderBottom(Collider col)
{
    if (col is SphereCollider sphere)
        return sphere.transform.position + sphere.center - sphere.radius * sphere.transform.up;
    if (col is CapsuleCollider capsule)
    {
        Vector3 dir = Vector3.up;
        switch (capsule.direction)
        {
            case 0: dir = capsule.transform.right; break;
            case 1: dir = capsule.transform.up; break;
            case 2: dir = capsule.transform.forward; break;
        }
        return capsule.transform.position + capsule.center - (capsule.height / 2f) * dir;
    }
    if (col is BoxCollider box)
        return box.transform.position + box.center - 0.5f * box.size.y * box.transform.up;

    return col.bounds.min;
}

private float GetColliderRadius(Collider col)
{
    if (col is SphereCollider sphere) return sphere.radius * _detectionRadiusScale;
    if (col is CapsuleCollider capsule) return Mathf.Max(capsule.radius, capsule.height / 2f) * _detectionRadiusScale;
    if (col is BoxCollider box) return Mathf.Max(box.size.x, box.size.z) / 2f * _detectionRadiusScale;
    return col.bounds.extents.x * _detectionRadiusScale;
}

    private Bounds GetCombinedBounds()
    {
        Bounds b = _colliders[0].bounds;
        for (int i = 1; i < _colliders.Length; i++)
            b.Encapsulate(_colliders[i].bounds);
        return b;
    }

    private bool IsOurCollider(Collider col)
    {
        for (int i = 0; i < _colliders.Length; i++)
            if (_colliders[i] == col) return true;
        return false;
    }

    private Vector3 GetGroundNormal(Collider groundCollider)
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            if (hit.collider == groundCollider)
            {
                TimeSinceLastGrounded = 0f;
                return hit.normal;
            }
        }

        TimeSinceLastGrounded += Time.fixedDeltaTime;
        return Vector3.up;
    }
}
