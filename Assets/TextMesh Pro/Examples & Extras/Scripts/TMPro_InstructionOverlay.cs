using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;


namespace TMPro.Examples
{
    
    public class TMProInstructionOverlay : MonoBehaviour
    {

        public enum FpsCounterAnchorPositions { TopLeft, BottomLeft, TopRight, BottomRight };

        [FormerlySerializedAs("AnchorPosition")] public FpsCounterAnchorPositions anchorPosition = FpsCounterAnchorPositions.BottomLeft;

        private const string Instructions = "Camera Control - <#ffff00>Shift + RMB\n</color>Zoom - <#ffff00>Mouse wheel.";

        private TextMeshPro mTextMeshPro;
        private TextContainer mTextContainer;
        private Transform mFrameCounterTransform;
        private Camera mCamera;

        //private FpsCounterAnchorPositions last_AnchorPosition;

        void Awake()
        {
            if (!enabled)
                return;

            mCamera = Camera.main;

            GameObject frameCounter = new GameObject("Frame Counter");
            mFrameCounterTransform = frameCounter.transform;
            mFrameCounterTransform.parent = mCamera.transform;
            mFrameCounterTransform.localRotation = Quaternion.identity;


            mTextMeshPro = frameCounter.AddComponent<TextMeshPro>();
            mTextMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            mTextMeshPro.fontSharedMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Overlay");

            mTextMeshPro.fontSize = 30;

            mTextMeshPro.isOverlay = true;
            mTextContainer = frameCounter.GetComponent<TextContainer>();

            Set_FrameCounter_Position(anchorPosition);
            //last_AnchorPosition = AnchorPosition;

            mTextMeshPro.text = Instructions;

        }




        void Set_FrameCounter_Position(FpsCounterAnchorPositions anchorPosition)
        {

            switch (anchorPosition)
            {
                case FpsCounterAnchorPositions.TopLeft:
                    //m_TextMeshPro.anchor = AnchorPositions.TopLeft;
                    mTextContainer.anchorPosition = TextContainerAnchors.TopLeft;
                    mFrameCounterTransform.position = mCamera.ViewportToWorldPoint(new Vector3(0, 1, 100.0f));
                    break;
                case FpsCounterAnchorPositions.BottomLeft:
                    //m_TextMeshPro.anchor = AnchorPositions.BottomLeft;
                    mTextContainer.anchorPosition = TextContainerAnchors.BottomLeft;
                    mFrameCounterTransform.position = mCamera.ViewportToWorldPoint(new Vector3(0, 0, 100.0f));
                    break;
                case FpsCounterAnchorPositions.TopRight:
                    //m_TextMeshPro.anchor = AnchorPositions.TopRight;
                    mTextContainer.anchorPosition = TextContainerAnchors.TopRight;
                    mFrameCounterTransform.position = mCamera.ViewportToWorldPoint(new Vector3(1, 1, 100.0f));
                    break;
                case FpsCounterAnchorPositions.BottomRight:
                    //m_TextMeshPro.anchor = AnchorPositions.BottomRight;
                    mTextContainer.anchorPosition = TextContainerAnchors.BottomRight;
                    mFrameCounterTransform.position = mCamera.ViewportToWorldPoint(new Vector3(1, 0, 100.0f));
                    break;
            }
        }
    }
}
