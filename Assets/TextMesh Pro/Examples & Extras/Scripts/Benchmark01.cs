using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;


namespace TMPro.Examples
{

    public class Benchmark01 : MonoBehaviour
    {

        [FormerlySerializedAs("BenchmarkType")] public int benchmarkType = 0;

        [FormerlySerializedAs("TMProFont")] public TMP_FontAsset tmProFont;
        [FormerlySerializedAs("TextMeshFont")] public Font textMeshFont;

        private TextMeshPro mTextMeshPro;
        private TextContainer mTextContainer;
        private TextMesh mTextMesh;

        private const string Label01 = "The <#0050FF>count is: </color>{0}";
        private const string Label02 = "The <color=#0050FF>count is: </color>";

        //private string m_string;
        //private int m_frame;

        private Material mMaterial01;
        private Material mMaterial02;



        IEnumerator Start()
        {



            if (benchmarkType == 0) // TextMesh Pro Component
            {
                mTextMeshPro = gameObject.AddComponent<TextMeshPro>();
                mTextMeshPro.autoSizeTextContainer = true;

                //m_textMeshPro.anchorDampening = true;

                if (tmProFont != null)
                    mTextMeshPro.font = tmProFont;

                //m_textMeshPro.font = Resources.Load("Fonts & Materials/Anton SDF", typeof(TextMeshProFont)) as TextMeshProFont; // Make sure the Anton SDF exists before calling this...
                //m_textMeshPro.fontSharedMaterial = Resources.Load("Fonts & Materials/Anton SDF", typeof(Material)) as Material; // Same as above make sure this material exists.

                mTextMeshPro.fontSize = 48;
                mTextMeshPro.alignment = TextAlignmentOptions.Center;
                //m_textMeshPro.anchor = AnchorPositions.Center;
                mTextMeshPro.extraPadding = true;
                //m_textMeshPro.outlineWidth = 0.25f;
                //m_textMeshPro.fontSharedMaterial.SetFloat("_OutlineWidth", 0.2f);
                //m_textMeshPro.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
                //m_textMeshPro.lineJustification = LineJustificationTypes.Center;
                mTextMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
                //m_textMeshPro.lineLength = 60;
                //m_textMeshPro.characterSpacing = 0.2f;
                //m_textMeshPro.fontColor = new Color32(255, 255, 255, 255);

                mMaterial01 = mTextMeshPro.font.material;
                mMaterial02 = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Drop Shadow"); // Make sure the LiberationSans SDF exists before calling this...


            }
            else if (benchmarkType == 1) // TextMesh
            {
                mTextMesh = gameObject.AddComponent<TextMesh>();

                if (textMeshFont != null)
                {
                    mTextMesh.font = textMeshFont;
                    mTextMesh.GetComponent<Renderer>().sharedMaterial = mTextMesh.font.material;
                }
                else
                {
                    mTextMesh.font = Resources.Load("Fonts/ARIAL", typeof(Font)) as Font;
                    mTextMesh.GetComponent<Renderer>().sharedMaterial = mTextMesh.font.material;
                }

                mTextMesh.fontSize = 48;
                mTextMesh.anchor = TextAnchor.MiddleCenter;

                //m_textMesh.color = new Color32(255, 255, 0, 255);
            }



            for (int i = 0; i <= 1000000; i++)
            {
                if (benchmarkType == 0)
                {
                    mTextMeshPro.SetText(Label01, i % 1000);
                    if (i % 1000 == 999)
                        mTextMeshPro.fontSharedMaterial = mTextMeshPro.fontSharedMaterial == mMaterial01 ? mTextMeshPro.fontSharedMaterial = mMaterial02 : mTextMeshPro.fontSharedMaterial = mMaterial01;



                }
                else if (benchmarkType == 1)
                    mTextMesh.text = Label02 + (i % 1000).ToString();

                yield return null;
            }


            yield return null;
        }


        /*
        void Update()
        {
            if (BenchmarkType == 0)
            {
                m_textMeshPro.text = (m_frame % 1000).ToString();
            }
            else if (BenchmarkType == 1)
            {
                m_textMesh.text = (m_frame % 1000).ToString();
            }

            m_frame += 1;
        }
        */
    }
}
