using UnityEngine;

namespace CreatureParts
{
    public class DeadBodyPart : MonoBehaviour
    {
        void OnCollisionStay(Collision col)
        {
            if (col.gameObject.layer == gameObject.layer)
            {
                float softness = 0.8f;
                foreach (ContactPoint contact in col.contacts)
                {
                    gameObject.GetComponent<Rigidbody>().AddForce(-contact.normal * col.impulse.magnitude * (1f - softness));
                }
            }
        }
    }
}
