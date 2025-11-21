namespace LifelikeMotion.IKFootPlacement
{
    using UnityEngine;
    using UnityEngine.Animations.Rigging;

    public class IKFootPlacement : MonoBehaviour
    {
        [Header("Two Bone IK Constraint")]
        public TwoBoneIKConstraint ikConstraint;

        [Header("Hips Transform")]
        public Transform hips;

        [Header("Raycast Settings")]
        public float raycastHeight = 0.5f;
        public float raycastLength = 1f;

        [Header("Feet Offset Weight")]
        [Range(0, 1)] public float feetPositionOffsetWeight = 1f;
        [Range(0, 1)] public float feetRotationOffsetWeight = 1f;
        
        [HideInInspector] public bool isGrounded = true;
        [HideInInspector] public bool jumped = false;
        [HideInInspector] public bool isMoving = true;

        private Animator animator;

        private void Awake()
        {
            if (hips == null)
            {
                Debug.LogError("Hips reference is missing!");
                enabled = false;
                return;
            }

            if (ikConstraint == null)
            {
                Debug.LogError("TwoBoneIKConstraint reference is missing!");
                enabled = false;
                return;
            }

            animator = hips.GetComponentInParent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator not found in parent hierarchy of hips!");
                enabled = false;
            }
        }

        private void Update()
        {
            if (!animator) return;

            RaycastHit hit;
            Vector3 origin = ikConstraint.data.target.position + Vector3.up * raycastHeight;

            if (Physics.Raycast(origin, Vector3.down, out hit, raycastLength))
            {
                Vector3 targetPos = hit.point;
                if (feetPositionOffsetWeight > 0)
                    ikConstraint.data.target.position = Vector3.Lerp(
                        ikConstraint.data.target.position, targetPos, feetPositionOffsetWeight);

                if (feetRotationOffsetWeight > 0)
                    ikConstraint.data.target.rotation = Quaternion.Lerp(
                        ikConstraint.data.target.rotation,
                        Quaternion.FromToRotation(Vector3.up, hit.normal) * ikConstraint.data.target.rotation,
                        feetRotationOffsetWeight);
            }
        }
    }
}
