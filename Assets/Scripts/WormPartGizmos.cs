using UnityEngine;
using UnityEngine.Serialization;

public class WormPartGizmos : MonoBehaviour
{
    public bool showGroundCollider = true;
    
    private SphereCollider _partCollider;
    private readonly float _verticalDetectionOffset = GameParameters.GroundingColliderVerticalDetectionOffset;
    
    void Start()
    {
        _partCollider = GetComponent<SphereCollider>();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (showGroundCollider)
        {
            if (_partCollider == null) _partCollider = GetComponent<SphereCollider>();

            Vector3 bottom = _partCollider.bounds.center - new Vector3(0, _partCollider.bounds.extents.y, 0);
            Vector3 checkPos = bottom + Vector3.down * _verticalDetectionOffset;
            float radius = _partCollider.bounds.extents.x * 0.9f;

            Gizmos.color = GetComponent<WormPart>().IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPos, radius);
        }
    }
}
