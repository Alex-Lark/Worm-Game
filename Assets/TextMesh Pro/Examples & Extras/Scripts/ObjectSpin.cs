using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;


namespace TMPro.Examples
{

    public class ObjectSpin : MonoBehaviour
    {
        #pragma warning disable 0414
        public enum MotionType { Rotation, SearchLight, Translation };
        [FormerlySerializedAs("Motion")] public MotionType motion;

        [FormerlySerializedAs("TranslationDistance")] public Vector3 translationDistance = new Vector3(5, 0, 0);
        [FormerlySerializedAs("TranslationSpeed")] public float translationSpeed = 1.0f;
        [FormerlySerializedAs("SpinSpeed")] public float spinSpeed = 5;
        [FormerlySerializedAs("RotationRange")] public int rotationRange = 15;
        private Transform mTransform;

        private float mTime;
        private Vector3 mPrevPos;
        private Vector3 mInitialRotation;
        private Vector3 mInitialPosition;
        private Color32 mLightColor;

        void Awake()
        {
            mTransform = transform;
            mInitialRotation = mTransform.rotation.eulerAngles;
            mInitialPosition = mTransform.position;

            Light light = GetComponent<Light>();
            mLightColor = light != null ? light.color : Color.black;
        }


        // Update is called once per frame
        void Update()
        {
            switch (motion)
            {
                case MotionType.Rotation:
                    mTransform.Rotate(0, spinSpeed * Time.deltaTime, 0);
                    break;
                case MotionType.SearchLight:
                    mTime += spinSpeed * Time.deltaTime;
                    mTransform.rotation = Quaternion.Euler(mInitialRotation.x, Mathf.Sin(mTime) * rotationRange + mInitialRotation.y, mInitialRotation.z);
                    break;
                case MotionType.Translation:
                    mTime += translationSpeed * Time.deltaTime;

                    float x = translationDistance.x * Mathf.Cos(mTime);
                    float y = translationDistance.y * Mathf.Sin(mTime) * Mathf.Cos(mTime * 1f);
                    float z = translationDistance.z * Mathf.Sin(mTime);

                    mTransform.position = mInitialPosition + new Vector3(x, z, y);

                    // Drawing light patterns because they can be cool looking.
                    //if (Time.frameCount > 1)
                    //    Debug.DrawLine(m_transform.position, m_prevPOS, m_lightColor, 100f);

                    mPrevPos = mTransform.position;
                    break;
            }
        }
    }
}