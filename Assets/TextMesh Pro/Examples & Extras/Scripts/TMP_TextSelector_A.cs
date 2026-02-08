using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;


namespace TMPro.Examples
{

    public class TMPTextSelectorA : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TextMeshPro mTextMeshPro;

        private Camera mCamera;

        private bool mIsHoveringObject;
        private int mSelectedLink = -1;
        private int mLastCharIndex = -1;
        private int mLastWordIndex = -1;

        void Awake()
        {
            mTextMeshPro = gameObject.GetComponent<TextMeshPro>();
            mCamera = Camera.main;

            // Force generation of the text object so we have valid data to work with. This is needed since LateUpdate() will be called before the text object has a chance to generated when entering play mode.
            mTextMeshPro.ForceMeshUpdate();
        }


        void LateUpdate()
        {
            mIsHoveringObject = false;

            if (TMP_TextUtilities.IsIntersectingRectTransform(mTextMeshPro.rectTransform, Input.mousePosition, Camera.main))
            {
                mIsHoveringObject = true;
            }

            if (mIsHoveringObject)
            {
                #region Example of Character Selection
                int charIndex = TMP_TextUtilities.FindIntersectingCharacter(mTextMeshPro, Input.mousePosition, Camera.main, true);
                if (charIndex != -1 && charIndex != mLastCharIndex && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    //Debug.Log("[" + m_TextMeshPro.textInfo.characterInfo[charIndex].character + "] has been selected.");

                    mLastCharIndex = charIndex;

                    int meshIndex = mTextMeshPro.textInfo.characterInfo[charIndex].materialReferenceIndex;

                    int vertexIndex = mTextMeshPro.textInfo.characterInfo[charIndex].vertexIndex;

                    Color32 c = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);

                    Color32[] vertexColors = mTextMeshPro.textInfo.meshInfo[meshIndex].colors32;

                    vertexColors[vertexIndex + 0] = c;
                    vertexColors[vertexIndex + 1] = c;
                    vertexColors[vertexIndex + 2] = c;
                    vertexColors[vertexIndex + 3] = c;

                    //m_TextMeshPro.mesh.colors32 = vertexColors;
                    mTextMeshPro.textInfo.meshInfo[meshIndex].mesh.colors32 = vertexColors;
                }
                #endregion

                #region Example of Link Handling
                // Check if mouse intersects with any links.
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(mTextMeshPro, Input.mousePosition, mCamera);

                // Clear previous link selection if one existed.
                if ((linkIndex == -1 && mSelectedLink != -1) || linkIndex != mSelectedLink)
                {
                    //m_TextPopup_RectTransform.gameObject.SetActive(false);
                    mSelectedLink = -1;
                }

                // Handle new Link selection.
                if (linkIndex != -1 && linkIndex != mSelectedLink)
                {
                    mSelectedLink = linkIndex;

                    TMP_LinkInfo linkInfo = mTextMeshPro.textInfo.linkInfo[linkIndex];

                    // The following provides an example of how to access the link properties.
                    //Debug.Log("Link ID: \"" + linkInfo.GetLinkID() + "\"   Link Text: \"" + linkInfo.GetLinkText() + "\""); // Example of how to retrieve the Link ID and Link Text.

                    Vector3 worldPointInRectangle;

                    RectTransformUtility.ScreenPointToWorldPointInRectangle(mTextMeshPro.rectTransform, Input.mousePosition, mCamera, out worldPointInRectangle);

                    switch (linkInfo.GetLinkID())
                    {
                        case "id_01": // 100041637: // id_01
                                      //m_TextPopup_RectTransform.position = worldPointInRectangle;
                                      //m_TextPopup_RectTransform.gameObject.SetActive(true);
                                      //m_TextPopup_TMPComponent.text = k_LinkText + " ID 01";
                            break;
                        case "id_02": // 100041638: // id_02
                                      //m_TextPopup_RectTransform.position = worldPointInRectangle;
                                      //m_TextPopup_RectTransform.gameObject.SetActive(true);
                                      //m_TextPopup_TMPComponent.text = k_LinkText + " ID 02";
                            break;
                    }
                }
                #endregion


                #region Example of Word Selection
                // Check if Mouse intersects any words and if so assign a random color to that word.
                int wordIndex = TMP_TextUtilities.FindIntersectingWord(mTextMeshPro, Input.mousePosition, Camera.main);
                if (wordIndex != -1 && wordIndex != mLastWordIndex)
                {
                    mLastWordIndex = wordIndex;

                    TMP_WordInfo wInfo = mTextMeshPro.textInfo.wordInfo[wordIndex];

                    Vector3 wordPos = mTextMeshPro.transform.TransformPoint(mTextMeshPro.textInfo.characterInfo[wInfo.firstCharacterIndex].bottomLeft);
                    wordPos = Camera.main.WorldToScreenPoint(wordPos);

                    //Debug.Log("Mouse Position: " + Input.mousePosition.ToString("f3") + "  Word Position: " + wordPOS.ToString("f3"));

                    Color32[] vertexColors = mTextMeshPro.textInfo.meshInfo[0].colors32;

                    Color32 c = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
                    for (int i = 0; i < wInfo.characterCount; i++)
                    {
                        int vertexIndex = mTextMeshPro.textInfo.characterInfo[wInfo.firstCharacterIndex + i].vertexIndex;

                        vertexColors[vertexIndex + 0] = c;
                        vertexColors[vertexIndex + 1] = c;
                        vertexColors[vertexIndex + 2] = c;
                        vertexColors[vertexIndex + 3] = c;
                    }

                    mTextMeshPro.mesh.colors32 = vertexColors;
                }
                #endregion
            }
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("OnPointerEnter()");
            mIsHoveringObject = true;
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("OnPointerExit()");
            mIsHoveringObject = false;
        }

    }
}
