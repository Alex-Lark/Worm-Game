using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;


namespace TMPro.Examples
{

    public class TextMeshProFloatingText : MonoBehaviour
    {
        [FormerlySerializedAs("TheFont")] public Font theFont;

        private GameObject mFloatingText;
        private TextMeshPro mTextMeshPro;
        private TextMesh mTextMesh;

        private Transform mTransform;
        private Transform mFloatingTextTransform;
        private Transform mCameraTransform;

        Vector3 lastPos = Vector3.zero;
        Quaternion lastRotation = Quaternion.identity;

        [FormerlySerializedAs("SpawnType")] public int spawnType;
        [FormerlySerializedAs("IsTextObjectScaleStatic")] public bool isTextObjectScaleStatic;

        //private int m_frame = 0;

        static WaitForEndOfFrame _kWaitForEndOfFrame = new WaitForEndOfFrame();
        static WaitForSeconds[] _kWaitForSecondsRandom = new WaitForSeconds[]
        {
            new WaitForSeconds(0.05f), new WaitForSeconds(0.1f), new WaitForSeconds(0.15f), new WaitForSeconds(0.2f), new WaitForSeconds(0.25f),
            new WaitForSeconds(0.3f), new WaitForSeconds(0.35f), new WaitForSeconds(0.4f), new WaitForSeconds(0.45f), new WaitForSeconds(0.5f),
            new WaitForSeconds(0.55f), new WaitForSeconds(0.6f), new WaitForSeconds(0.65f), new WaitForSeconds(0.7f), new WaitForSeconds(0.75f),
            new WaitForSeconds(0.8f), new WaitForSeconds(0.85f), new WaitForSeconds(0.9f), new WaitForSeconds(0.95f), new WaitForSeconds(1.0f),
        };

        void Awake()
        {
            mTransform = transform;
            mFloatingText = new GameObject(this.name + " floating text");

            // Reference to Transform is lost when TMP component is added since it replaces it by a RectTransform.
            //m_floatingText_Transform = m_floatingText.transform;
            //m_floatingText_Transform.position = m_transform.position + new Vector3(0, 15f, 0);

            mCameraTransform = Camera.main.transform;
        }

        void Start()
        {
            if (spawnType == 0)
            {
                // TextMesh Pro Implementation
                mTextMeshPro = mFloatingText.AddComponent<TextMeshPro>();
                mTextMeshPro.rectTransform.sizeDelta = new Vector2(3, 3);

                mFloatingTextTransform = mFloatingText.transform;
                mFloatingTextTransform.position = mTransform.position + new Vector3(0, 15f, 0);

                //m_textMeshPro.fontAsset = Resources.Load("Fonts & Materials/JOKERMAN SDF", typeof(TextMeshProFont)) as TextMeshProFont; // User should only provide a string to the resource.
                //m_textMeshPro.fontSharedMaterial = Resources.Load("Fonts & Materials/LiberationSans SDF", typeof(Material)) as Material;

                mTextMeshPro.alignment = TextAlignmentOptions.Center;
                mTextMeshPro.color = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
                mTextMeshPro.fontSize = 24;
                //m_textMeshPro.enableExtraPadding = true;
                //m_textMeshPro.enableShadows = false;
                mTextMeshPro.fontFeatures.Clear();
                mTextMeshPro.text = string.Empty;
                mTextMeshPro.isTextObjectScaleStatic = isTextObjectScaleStatic;

                StartCoroutine(DisplayTextMeshProFloatingText());
            }
            else if (spawnType == 1)
            {
                //Debug.Log("Spawning TextMesh Objects.");

                mFloatingTextTransform = mFloatingText.transform;
                mFloatingTextTransform.position = mTransform.position + new Vector3(0, 15f, 0);

                mTextMesh = mFloatingText.AddComponent<TextMesh>();
                mTextMesh.font = Resources.Load<Font>("Fonts/ARIAL");
                mTextMesh.GetComponent<Renderer>().sharedMaterial = mTextMesh.font.material;
                mTextMesh.color = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
                mTextMesh.anchor = TextAnchor.LowerCenter;
                mTextMesh.fontSize = 24;

                StartCoroutine(DisplayTextMeshFloatingText());
            }
            else if (spawnType == 2)
            {

            }

        }


        //void Update()
        //{
        //    if (SpawnType == 0)
        //    {
        //        m_textMeshPro.SetText("{0}", m_frame);
        //    }
        //    else
        //    {
        //        m_textMesh.text = m_frame.ToString();
        //    }
        //    m_frame = (m_frame + 1) % 1000;

        //}


        public IEnumerator DisplayTextMeshProFloatingText()
        {
            float countDuration = 2.0f; // How long is the countdown alive.
            float startingCount = Random.Range(5f, 20f); // At what number is the counter starting at.
            float currentCount = startingCount;

            Vector3 startPos = mFloatingTextTransform.position;
            Color32 startColor = mTextMeshPro.color;
            float alpha = 255;
            int intCounter = 0;


            float fadeDuration = 3 / startingCount * countDuration;

            while (currentCount > 0)
            {
                currentCount -= (Time.deltaTime / countDuration) * startingCount;

                if (currentCount <= 3)
                {
                    //Debug.Log("Fading Counter ... " + current_Count.ToString("f2"));
                    alpha = Mathf.Clamp(alpha - (Time.deltaTime / fadeDuration) * 255, 0, 255);
                }

                intCounter = (int)currentCount;
                mTextMeshPro.text = intCounter.ToString();
                //m_textMeshPro.SetText("{0}", (int)current_Count);

                mTextMeshPro.color = new Color32(startColor.r, startColor.g, startColor.b, (byte)alpha);

                // Move the floating text upward each update
                mFloatingTextTransform.position += new Vector3(0, startingCount * Time.deltaTime, 0);

                // Align floating text perpendicular to Camera.
                if (!lastPos.Compare(mCameraTransform.position, 1000) || !lastRotation.Compare(mCameraTransform.rotation, 1000))
                {
                    lastPos = mCameraTransform.position;
                    lastRotation = mCameraTransform.rotation;
                    mFloatingTextTransform.rotation = lastRotation;
                    Vector3 dir = mTransform.position - lastPos;
                    mTransform.forward = new Vector3(dir.x, 0, dir.z);
                }

                yield return _kWaitForEndOfFrame;
            }

            //Debug.Log("Done Counting down.");

            yield return _kWaitForSecondsRandom[Random.Range(0, 19)];

            mFloatingTextTransform.position = startPos;

            StartCoroutine(DisplayTextMeshProFloatingText());
        }


        public IEnumerator DisplayTextMeshFloatingText()
        {
            float countDuration = 2.0f; // How long is the countdown alive.
            float startingCount = Random.Range(5f, 20f); // At what number is the counter starting at.
            float currentCount = startingCount;

            Vector3 startPos = mFloatingTextTransform.position;
            Color32 startColor = mTextMesh.color;
            float alpha = 255;
            int intCounter = 0;

            float fadeDuration = 3 / startingCount * countDuration;

            while (currentCount > 0)
            {
                currentCount -= (Time.deltaTime / countDuration) * startingCount;

                if (currentCount <= 3)
                {
                    //Debug.Log("Fading Counter ... " + current_Count.ToString("f2"));
                    alpha = Mathf.Clamp(alpha - (Time.deltaTime / fadeDuration) * 255, 0, 255);
                }

                intCounter = (int)currentCount;
                mTextMesh.text = intCounter.ToString();
                //Debug.Log("Current Count:" + current_Count.ToString("f2"));

                mTextMesh.color = new Color32(startColor.r, startColor.g, startColor.b, (byte)alpha);

                // Move the floating text upward each update
                mFloatingTextTransform.position += new Vector3(0, startingCount * Time.deltaTime, 0);

                // Align floating text perpendicular to Camera.
                if (!lastPos.Compare(mCameraTransform.position, 1000) || !lastRotation.Compare(mCameraTransform.rotation, 1000))
                {
                    lastPos = mCameraTransform.position;
                    lastRotation = mCameraTransform.rotation;
                    mFloatingTextTransform.rotation = lastRotation;
                    Vector3 dir = mTransform.position - lastPos;
                    mTransform.forward = new Vector3(dir.x, 0, dir.z);
                }

                yield return _kWaitForEndOfFrame;
            }

            //Debug.Log("Done Counting down.");

            yield return _kWaitForSecondsRandom[Random.Range(0, 20)];

            mFloatingTextTransform.position = startPos;

            StartCoroutine(DisplayTextMeshFloatingText());
        }
    }
}
