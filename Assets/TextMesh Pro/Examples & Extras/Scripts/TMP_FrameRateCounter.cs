using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;


namespace TMPro.Examples
{

    public class TMPFrameRateCounter : MonoBehaviour
    {
        [FormerlySerializedAs("UpdateInterval")] public float updateInterval = 5.0f;
        private float mLastInterval = 0;
        private int mFrames = 0;

        public enum FpsCounterAnchorPositions { TopLeft, BottomLeft, TopRight, BottomRight };

        [FormerlySerializedAs("AnchorPosition")] public FpsCounterAnchorPositions anchorPosition = FpsCounterAnchorPositions.TopRight;

        private string htmlColorTag;
        private const string FPSLabel = "{0:2}</color> <#8080ff>FPS \n<#FF8000>{1:2} <#8080ff>MS";

        private TextMeshPro mTextMeshPro;
        private Transform mFrameCounterTransform;
        private Camera mCamera;

        private FpsCounterAnchorPositions lastAnchorPosition;

        void Awake()
        {
            if (!enabled)
                return;

            mCamera = Camera.main;
            Application.targetFrameRate = 9999;

            GameObject frameCounter = new GameObject("Frame Counter");

            mTextMeshPro = frameCounter.AddComponent<TextMeshPro>();
            mTextMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            mTextMeshPro.fontSharedMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Overlay");


            mFrameCounterTransform = frameCounter.transform;
            mFrameCounterTransform.SetParent(mCamera.transform);
            mFrameCounterTransform.localRotation = Quaternion.identity;

            mTextMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
            mTextMeshPro.fontSize = 24;
            //m_TextMeshPro.FontColor = new Color32(255, 255, 255, 128);
            //m_TextMeshPro.edgeWidth = .15f;
            //m_TextMeshPro.isOverlay = true;

            //m_TextMeshPro.FaceColor = new Color32(255, 128, 0, 0);
            //m_TextMeshPro.EdgeColor = new Color32(0, 255, 0, 255);
            //m_TextMeshPro.FontMaterial.renderQueue = 4000;

            //m_TextMeshPro.CreateSoftShadowClone(new Vector2(1f, -1f));

            Set_FrameCounter_Position(anchorPosition);
            lastAnchorPosition = anchorPosition;


        }

        void Start()
        {
            mLastInterval = Time.realtimeSinceStartup;
            mFrames = 0;
        }

        void Update()
        {
            if (anchorPosition != lastAnchorPosition)
                Set_FrameCounter_Position(anchorPosition);

            lastAnchorPosition = anchorPosition;

            mFrames += 1;
            float timeNow = Time.realtimeSinceStartup;

            if (timeNow > mLastInterval + updateInterval)
            {
                // display two fractional digits (f2 format)
                float fps = mFrames / (timeNow - mLastInterval);
                float ms = 1000.0f / Mathf.Max(fps, 0.00001f);

                if (fps < 30)
                    htmlColorTag = "<color=yellow>";
                else if (fps < 10)
                    htmlColorTag = "<color=red>";
                else
                    htmlColorTag = "<color=green>";

                //string format = System.String.Format(htmlColorTag + "{0:F2} </color>FPS \n{1:F2} <#8080ff>MS",fps, ms);
                //m_TextMeshPro.text = format;

                mTextMeshPro.SetText(htmlColorTag + FPSLabel, fps, ms);

                mFrames = 0;
                mLastInterval = timeNow;
            }
        }


        void Set_FrameCounter_Position(FpsCounterAnchorPositions anchorPosition)
        {
            //Debug.Log("Changing frame counter anchor position.");
            mTextMeshPro.margin = new Vector4(1f, 1f, 1f, 1f);

            switch (anchorPosition)
            {
                case FpsCounterAnchorPositions.TopLeft:
                    mTextMeshPro.alignment = TextAlignmentOptions.TopLeft;
                    mTextMeshPro.rectTransform.pivot = new Vector2(0, 1);
                    mFrameCounterTransform.position = mCamera.ViewportToWorldPoint(new Vector3(0, 1, 100.0f));
                    break;
                case FpsCounterAnchorPositions.BottomLeft:
                    mTextMeshPro.alignment = TextAlignmentOptions.BottomLeft;
                    mTextMeshPro.rectTransform.pivot = new Vector2(0, 0);
                    mFrameCounterTransform.position = mCamera.ViewportToWorldPoint(new Vector3(0, 0, 100.0f));
                    break;
                case FpsCounterAnchorPositions.TopRight:
                    mTextMeshPro.alignment = TextAlignmentOptions.TopRight;
                    mTextMeshPro.rectTransform.pivot = new Vector2(1, 1);
                    mFrameCounterTransform.position = mCamera.ViewportToWorldPoint(new Vector3(1, 1, 100.0f));
                    break;
                case FpsCounterAnchorPositions.BottomRight:
                    mTextMeshPro.alignment = TextAlignmentOptions.BottomRight;
                    mTextMeshPro.rectTransform.pivot = new Vector2(1, 0);
                    mFrameCounterTransform.position = mCamera.ViewportToWorldPoint(new Vector3(1, 0, 100.0f));
                    break;
            }
        }
    }
}
