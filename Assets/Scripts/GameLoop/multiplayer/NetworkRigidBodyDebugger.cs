// Attach this temporarily to any GameObject in the scene

using UnityEngine;

public class NetworkRigidbodyDebugger : MonoBehaviour
{
    private void Update()
    {
        var allNRBs = FindObjectsByType<PurrNet.NetworkRigidbody>(
            FindObjectsInactive.Include, 
            FindObjectsSortMode.None
        );
        
        foreach (var nrb in allNRBs)
        {
            var rb = nrb.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError($"[NRB] Missing Rigidbody on: {GetFullPath(nrb.transform)}", nrb.gameObject);
                continue;
            }
            
            if (rb.isKinematic)
            {
                Debug.LogWarning($"[NRB] Rigidbody is kinematic on: {GetFullPath(nrb.transform)}", nrb.gameObject);
            }
            
            if (!nrb.isActiveAndEnabled)
            {
                Debug.LogWarning($"[NRB] NetworkRigidbody disabled on: {GetFullPath(nrb.transform)}", nrb.gameObject);
            }

            if (!nrb.isSpawned)
            {
                Debug.LogWarning($"[NRB] Not spawned: {GetFullPath(nrb.transform)}", nrb.gameObject);
            }
        }
    }
    
    private string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}