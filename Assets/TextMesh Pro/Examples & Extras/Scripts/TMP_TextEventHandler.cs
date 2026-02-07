using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Serialization;


namespace TMPro
{

    public class TMPTextEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        public class CharacterSelectionEvent : UnityEvent<char, int> { }

        [Serializable]
        public class SpriteSelectionEvent : UnityEvent<char, int> { }

        [Serializable]
        public class WordSelectionEvent : UnityEvent<string, int, int> { }

        [Serializable]
        public class LineSelectionEvent : UnityEvent<string, int, int> { }

        [Serializable]
        public class LinkSelectionEvent : UnityEvent<string, string, int> { }


        /// <summary>
        /// Event delegate triggered when pointer is over a character.
        /// </summary>
        public CharacterSelectionEvent OnCharacterSelection
        {
            get { return mOnCharacterSelection; }
            set { mOnCharacterSelection = value; }
        }
        [FormerlySerializedAs("m_OnCharacterSelection")] [SerializeField]
        private CharacterSelectionEvent mOnCharacterSelection = new CharacterSelectionEvent();


        /// <summary>
        /// Event delegate triggered when pointer is over a sprite.
        /// </summary>
        public SpriteSelectionEvent OnSpriteSelection
        {
            get { return mOnSpriteSelection; }
            set { mOnSpriteSelection = value; }
        }
        [FormerlySerializedAs("m_OnSpriteSelection")] [SerializeField]
        private SpriteSelectionEvent mOnSpriteSelection = new SpriteSelectionEvent();


        /// <summary>
        /// Event delegate triggered when pointer is over a word.
        /// </summary>
        public WordSelectionEvent OnWordSelection
        {
            get { return mOnWordSelection; }
            set { mOnWordSelection = value; }
        }
        [FormerlySerializedAs("m_OnWordSelection")] [SerializeField]
        private WordSelectionEvent mOnWordSelection = new WordSelectionEvent();


        /// <summary>
        /// Event delegate triggered when pointer is over a line.
        /// </summary>
        public LineSelectionEvent OnLineSelection
        {
            get { return mOnLineSelection; }
            set { mOnLineSelection = value; }
        }
        [FormerlySerializedAs("m_OnLineSelection")] [SerializeField]
        private LineSelectionEvent mOnLineSelection = new LineSelectionEvent();


        /// <summary>
        /// Event delegate triggered when pointer is over a link.
        /// </summary>
        public LinkSelectionEvent OnLinkSelection
        {
            get { return mOnLinkSelection; }
            set { mOnLinkSelection = value; }
        }
        [FormerlySerializedAs("m_OnLinkSelection")] [SerializeField]
        private LinkSelectionEvent mOnLinkSelection = new LinkSelectionEvent();



        private TMP_Text mTextComponent;

        private Camera mCamera;
        private Canvas mCanvas;

        private int mSelectedLink = -1;
        private int mLastCharIndex = -1;
        private int mLastWordIndex = -1;
        private int mLastLineIndex = -1;

        void Awake()
        {
            // Get a reference to the text component.
            mTextComponent = gameObject.GetComponent<TMP_Text>();

            // Get a reference to the camera rendering the text taking into consideration the text component type.
            if (mTextComponent.GetType() == typeof(TextMeshProUGUI))
            {
                mCanvas = gameObject.GetComponentInParent<Canvas>();
                if (mCanvas != null)
                {
                    if (mCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        mCamera = null;
                    else
                        mCamera = mCanvas.worldCamera;
                }
            }
            else
            {
                mCamera = Camera.main;
            }
        }


        void LateUpdate()
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(mTextComponent.rectTransform, Input.mousePosition, mCamera))
            {
                #region Nearest Character
                /*int charIndex = TMP_TextUtilities.FindNearestCharacterOnLine(m_TextComponent, Input.mousePosition, 0, m_Camera, false);
                if (charIndex != -1 && charIndex != m_lastCharIndex)
                {
                    m_lastCharIndex = charIndex;
                }*/
                #endregion


                #region Example of Character or Sprite Selection
                int charIndex = TMP_TextUtilities.FindIntersectingCharacter(mTextComponent, Input.mousePosition, mCamera, true);
                if (charIndex != -1 && charIndex != mLastCharIndex)
                {
                    mLastCharIndex = charIndex;

                    TMP_TextElementType elementType = mTextComponent.textInfo.characterInfo[charIndex].elementType;

                    // Send event to any event listeners depending on whether it is a character or sprite.
                    if (elementType == TMP_TextElementType.Character)
                        SendOnCharacterSelection(mTextComponent.textInfo.characterInfo[charIndex].character, charIndex);
                    else if (elementType == TMP_TextElementType.Sprite)
                        SendOnSpriteSelection(mTextComponent.textInfo.characterInfo[charIndex].character, charIndex);
                }
                #endregion


                #region Example of Word Selection
                // Check if Mouse intersects any words and if so assign a random color to that word.
                int wordIndex = TMP_TextUtilities.FindIntersectingWord(mTextComponent, Input.mousePosition, mCamera);
                if (wordIndex != -1 && wordIndex != mLastWordIndex)
                {
                    mLastWordIndex = wordIndex;

                    // Get the information about the selected word.
                    TMP_WordInfo wInfo = mTextComponent.textInfo.wordInfo[wordIndex];

                    // Send the event to any listeners.
                    SendOnWordSelection(wInfo.GetWord(), wInfo.firstCharacterIndex, wInfo.characterCount);
                }
                #endregion


                #region Example of Line Selection
                // Check if Mouse intersects any words and if so assign a random color to that word.
                int lineIndex = TMP_TextUtilities.FindIntersectingLine(mTextComponent, Input.mousePosition, mCamera);
                if (lineIndex != -1 && lineIndex != mLastLineIndex)
                {
                    mLastLineIndex = lineIndex;

                    // Get the information about the selected word.
                    TMP_LineInfo lineInfo = mTextComponent.textInfo.lineInfo[lineIndex];

                    // Send the event to any listeners.
                    char[] buffer = new char[lineInfo.characterCount];
                    for (int i = 0; i < lineInfo.characterCount && i < mTextComponent.textInfo.characterInfo.Length; i++)
                    {
                        buffer[i] = mTextComponent.textInfo.characterInfo[i + lineInfo.firstCharacterIndex].character;
                    }

                    string lineText = new string(buffer);
                    SendOnLineSelection(lineText, lineInfo.firstCharacterIndex, lineInfo.characterCount);
                }
                #endregion


                #region Example of Link Handling
                // Check if mouse intersects with any links.
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(mTextComponent, Input.mousePosition, mCamera);

                // Handle new Link selection.
                if (linkIndex != -1 && linkIndex != mSelectedLink)
                {
                    mSelectedLink = linkIndex;

                    // Get information about the link.
                    TMP_LinkInfo linkInfo = mTextComponent.textInfo.linkInfo[linkIndex];

                    // Send the event to any listeners.
                    SendOnLinkSelection(linkInfo.GetLinkID(), linkInfo.GetLinkText(), linkIndex);
                }
                #endregion
            }
            else
            {
                // Reset all selections given we are hovering outside the text container bounds.
                mSelectedLink = -1;
                mLastCharIndex = -1;
                mLastWordIndex = -1;
                mLastLineIndex = -1;
            }
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("OnPointerEnter()");
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("OnPointerExit()");
        }


        private void SendOnCharacterSelection(char character, int characterIndex)
        {
            if (OnCharacterSelection != null)
                OnCharacterSelection.Invoke(character, characterIndex);
        }

        private void SendOnSpriteSelection(char character, int characterIndex)
        {
            if (OnSpriteSelection != null)
                OnSpriteSelection.Invoke(character, characterIndex);
        }

        private void SendOnWordSelection(string word, int charIndex, int length)
        {
            if (OnWordSelection != null)
                OnWordSelection.Invoke(word, charIndex, length);
        }

        private void SendOnLineSelection(string line, int charIndex, int length)
        {
            if (OnLineSelection != null)
                OnLineSelection.Invoke(line, charIndex, length);
        }

        private void SendOnLinkSelection(string linkID, string linkText, int linkIndex)
        {
            if (OnLinkSelection != null)
                OnLinkSelection.Invoke(linkID, linkText, linkIndex);
        }

    }
}
