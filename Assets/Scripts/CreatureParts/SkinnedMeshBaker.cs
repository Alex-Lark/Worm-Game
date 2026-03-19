using System;
using UnityEngine;

namespace CreatureParts
{
    public class SkinnedMeshBaker : MonoBehaviour
    {
        public enum BakeMode
        {
            OnceOnStart, // Bake once, never update
            EveryAnimationFrame // Rebake when animation advances
        }

        [Header("Settings")] public BakeMode bakeMode = BakeMode.OnceOnStart;
        public bool bakeCollider = true;
        public bool bakeOutlineMesh = false;
        public Vector3 meshScale = Vector3.one;

        [Header("References (auto-found if empty)")]
        public SkinnedMeshRenderer skinnedMeshRenderer;

        public Animator animator;

        private MeshCollider _meshCollider;
        private MeshFilter _outlineMeshFilter;
        private int _lastAnimatorHash;

        private void Awake()
        {
            if (skinnedMeshRenderer == null)
                skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (skinnedMeshRenderer == null)
            {
                Debug.LogWarning($"[SkinnedMeshBaker] No SkinnedMeshRenderer found on {gameObject.name}");
                enabled = false;
                return;
            }

            if (bakeCollider)
            {
                // Collider goes on THIS object (the root), not the skinned mesh child
                _meshCollider = GetComponent<MeshCollider>();
                if (_meshCollider == null)
                    _meshCollider = gameObject.AddComponent<MeshCollider>();
            }

            Bake();

            if (bakeMode == BakeMode.OnceOnStart)
                enabled = false;
        }

        public void EnableConvex()
        {
            //TODO: call this from creatureBuilding after multiplayer has been merged
            _meshCollider.convex = true;
        }

        public void DisableConvex()
        {
            //TODO: call this from creatureBuilding after multiplayer has been merged
            _meshCollider.convex = false;
        }

        public void Bake()
        {
            if (skinnedMeshRenderer == null) return;

            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            Debug.Log($"RAW baked bounds: {bakedMesh.bounds}, first vertex: {bakedMesh.vertices[0]}");

            // BakeMesh outputs verts in SMR local space with scale already applied
            // We need to: rotate by SMR world rotation, then inverse rotate by root rotation
            // Position offset between SMR and root (they're the same here so this is zero)
            Quaternion smrRot = skinnedMeshRenderer.transform.rotation;
            Quaternion rootInvRot = Quaternion.Inverse(transform.rotation);
            Vector3 posOffset = transform.InverseTransformPoint(skinnedMeshRenderer.transform.position);

            Vector3[] vertices = bakedMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = rootInvRot * (smrRot * vertices[i]) + posOffset;
                // Apply scale after rotation
                vertices[i].x *= meshScale.x;
                vertices[i].y *= meshScale.y;
                vertices[i].z *= meshScale.z;
            }
            bakedMesh.vertices = vertices;
            bakedMesh.RecalculateBounds();
            bakedMesh.RecalculateNormals();

            Debug.Log($"[SkinnedMeshBaker] After transform - bounds: {bakedMesh.bounds}");

            if (bakeCollider && _meshCollider != null)
            {
                _meshCollider.sharedMesh = null;
                _meshCollider.sharedMesh = bakedMesh;
                Debug.Log($"[SkinnedMeshBaker] Collider world bounds: {_meshCollider.bounds}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            var col = GetComponent<MeshCollider>();
            if (col == null || col.sharedMesh == null) return;

            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireMesh(col.sharedMesh);
        }
        
        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            if (skinnedMeshRenderer == null)
                skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (_meshCollider == null)
                _meshCollider = GetComponent<MeshCollider>();
            Bake();
        }
    }
}