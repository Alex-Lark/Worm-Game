using UnityEngine;

public class WormPart : MonoBehaviour
{
    public bool IsGrounded { get; private set; }

    private SphereCollider partCollider;
    [SerializeField] private float verticalDetectionOffset = 0.15f; // Increased offset
    [SerializeField] private float detectionRadiusScale = 0.5f; // Smaller radius

    private void Awake()
    {
        partCollider = GetComponent<SphereCollider>();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
    }

    private void CheckGrounded()
    {
        Vector3 bottom = partCollider.bounds.center - new Vector3(0, partCollider.bounds.extents.y, 0);
        Vector3 checkPos = bottom + Vector3.down * verticalDetectionOffset;
        float radius = partCollider.bounds.extents.x * detectionRadiusScale; // Much smaller

        Collider[] hits = Physics.OverlapSphere(checkPos, radius, ~0, QueryTriggerInteraction.Ignore);
        IsGrounded = false;
        
        foreach (var hit in hits)
        {
            if (hit != partCollider)
            {
                IsGrounded = true;
                Debug.Log($"{gameObject.name}: GROUNDED by {hit.gameObject.name}");
                break;
            }
        }
        
        if (!IsGrounded)
        {
            Debug.Log($"{gameObject.name}: AIRBORNE");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (partCollider == null) partCollider = GetComponent<SphereCollider>();

        Vector3 bottom = partCollider.bounds.center - new Vector3(0, partCollider.bounds.extents.y, 0);
        Vector3 checkPos = bottom + Vector3.down * verticalDetectionOffset;
        float radius = partCollider.bounds.extents.x * detectionRadiusScale;

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPos, radius);
    }
}