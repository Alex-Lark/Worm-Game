using System.Collections.Generic;

namespace LifelikeMotion.IKFootPlacement
{
    using UnityEngine;
    using UnityEngine.Animations.Rigging;

    public class IKSetup : MonoBehaviour
    {
        [Header("Auto-detected Leg Bones")]
        [Tooltip("Upper leg bone (thigh) - Will auto-detect from children")]
        public Transform upLeg;

        [Tooltip("Lower leg bone (shin) - Will auto-detect from children")]
        public Transform leg;

        [Tooltip("Foot bone - Will auto-detect from children")]
        public Transform foot;

        [Header("Bone Name Patterns (for auto-detection)")]
        [Tooltip("Keywords to identify upper leg bone (e.g., 'UpLeg', 'Thigh')")]
        public string upLegPattern = "UpLeg";

        [Tooltip("Keywords to identify lower leg bone (e.g., 'Leg', 'Shin', 'Calf')")]
        public string legPattern = "Leg";

        [Tooltip("Keywords to identify foot bone (e.g., 'Foot', 'Ankle')")]
        public string footPattern = "Foot";

        // Legacy fields for editor compatibility (kept for IKSetupGUI.cs)
        [HideInInspector] public Transform hips;
        [HideInInspector] public List<IKLeg> leftLegsTransforms = new List<IKLeg>();
        [HideInInspector] public List<IKLeg> rightLegsTransforms = new List<IKLeg>();
        [HideInInspector] public string hipsName = "Hips";
        [HideInInspector] public string leftUpLegName = "LeftUpLeg";
        [HideInInspector] public string leftLegName = "LeftLeg";
        [HideInInspector] public string leftFootName = "LeftFoot";
        [HideInInspector] public string rightUpLegName = "RightUpLeg";
        [HideInInspector] public string rightLegName = "RightLeg";
        [HideInInspector] public string rightFootName = "RightFoot";

        private Transform characterRoot;
        private RigBuilder rigBuilder;
        private Rig rig;

        private void OnValidate()
        {
            // Auto-detect leg bones when attached to a GameObject
            if (upLeg == null || leg == null || foot == null)
            {
                FindReferences();
            }
        }

        public bool SetupIKRig()
        {
            // Find references first if not set
            if (upLeg == null || leg == null || foot == null)
            {
                FindReferences();
            }

            #region Safety Checks
            if (upLeg == null || leg == null || foot == null)
            {
                Debug.LogError("Could not detect leg bones. Make sure this script is attached to a GameObject that contains or is near the leg bones in the hierarchy. Check the bone name patterns in the inspector.");
                return false;
            }

            if (characterRoot == null)
            {
                Debug.LogError("Could not find character root with Animator component. Please ensure your character has an Animator component.");
                return false;
            }
            #endregion

            #region Adding Rig Builder and Rig components to Character Root
            try
            {
                rigBuilder = characterRoot.GetComponent<RigBuilder>();
                if (rigBuilder == null)
                {
                    rigBuilder = characterRoot.gameObject.AddComponent<RigBuilder>();
                    Debug.Log("Added Rig Builder component to character root!");
                }

                rig = characterRoot.GetComponent<Rig>();
                if (rig == null)
                {
                    rig = characterRoot.gameObject.AddComponent<Rig>();
                    Debug.Log("Added Rig component to character root!");
                }
            }
            catch
            {
                Debug.LogError("Could not add Rig Builder and Rig components to the character root. Please try running the script again or add them manually.");
                return false;
            }
            #endregion

            #region Building Rig Builder
            try
            {
                if (rigBuilder.layers.Count == 0 || rigBuilder.layers[0].rig != rig)
                {
                    rigBuilder.layers.Clear();
                    rigBuilder.layers.Add(new RigLayer(rig, true));
                }
                rigBuilder.enabled = true;
            }
            catch
            {
                Debug.LogError("Could not implement newly added Rig Builder component. Please read console log and try running the script again.");
                return false;
            }
            #endregion

            #region Creating IK Controls
            string legName = gameObject.name;
            
            // Create IK hierarchy on character root
            Transform ikRootTransform = characterRoot.Find("IK_Controls");
            GameObject ikRoot;
            
            if (ikRootTransform == null)
            {
                ikRoot = new GameObject("IK_Controls");
                ikRoot.transform.parent = characterRoot;
                ikRoot.transform.localPosition = Vector3.zero;
            }
            else
            {
                ikRoot = ikRootTransform.gameObject;
            }

            GameObject legIKGroup = new GameObject($"IK_{legName}");
            legIKGroup.transform.parent = ikRoot.transform;
            legIKGroup.transform.localPosition = Vector3.zero;

            GameObject ikConstraintObj = new GameObject($"{legName}_IKConstraint");
            ikConstraintObj.transform.parent = legIKGroup.transform;

            GameObject targetObj = new GameObject($"{legName}_Target");
            targetObj.transform.parent = legIKGroup.transform;
            targetObj.transform.position = foot.position;
            targetObj.transform.rotation = foot.rotation;

            GameObject hintObj = new GameObject($"{legName}_Hint");
            hintObj.transform.parent = legIKGroup.transform;
            hintObj.transform.position = leg.position;

            Debug.Log($"Created IK Controls for {legName}!");
            #endregion

            #region Adding IK Constraint Component
            TwoBoneIKConstraint twoBoneIK = ikConstraintObj.AddComponent<TwoBoneIKConstraint>();
            
            twoBoneIK.data.root = upLeg;
            twoBoneIK.data.mid = leg;
            twoBoneIK.data.tip = foot;
            twoBoneIK.data.target = targetObj.transform;
            twoBoneIK.data.hint = hintObj.transform;
            twoBoneIK.data.maintainTargetRotationOffset = true;
            
            // Add IKFootPlacement immediately after
            var footPlacement = gameObject.AddComponent<Packages.Lifelike_Motion.IKFootPlacement___Free_IK_Script.Tool.IKFootPlacement>();
            footPlacement.hips = upLeg; // since it contains Animator
            footPlacement.ikConstraint = twoBoneIK;
            footPlacement.enabled = true;

            Debug.Log("Added Two Bone IK Constraint component!");
            #endregion

            #region Linking Constraint to Rig
            // Create constraints folder under rig if it doesn't exist
            Transform constraintsFolder = rig.transform.Find("Constraints");
            if (constraintsFolder == null)
            {
                GameObject constraintsObj = new GameObject("Constraints");
                constraintsObj.transform.parent = rig.transform;
                constraintsFolder = constraintsObj.transform;
            }

            ikConstraintObj.transform.parent = constraintsFolder;
            #endregion

            Debug.Log($"Setup finished successfully for {legName}!");
            return true;
        }

        public void FindReferences()
        {
            // Search for leg bones in this GameObject and its children
            Transform[] allTransforms = GetComponentsInChildren<Transform>();

            // Find upper leg
            foreach (Transform t in allTransforms)
            {
                if (t.name.Contains(upLegPattern) && upLeg == null)
                {
                    upLeg = t;
                    break;
                }
            }

            // Find lower leg (should be child of upper leg)
            if (upLeg != null)
            {
                foreach (Transform t in upLeg.GetComponentsInChildren<Transform>())
                {
                    if (t != upLeg && t.name.Contains(legPattern) && !t.name.Contains(upLegPattern) && leg == null)
                    {
                        leg = t;
                        break;
                    }
                }
            }

            // Find foot (should be child of lower leg)
            if (leg != null)
            {
                foreach (Transform t in leg.GetComponentsInChildren<Transform>())
                {
                    if (t != leg && t.name.Contains(footPattern) && foot == null)
                    {
                        foot = t;
                        break;
                    }
                }
            }

            // Find character root by searching up the hierarchy for Animator
            Transform current = transform;
            while (current != null)
            {
                if (current.GetComponent<Animator>() != null)
                {
                    characterRoot = current;
                    Debug.Log($"Found character root: {characterRoot.name}");
                    break;
                }
                current = current.parent;
            }

            // Validate references
            if (upLeg != null && leg != null && foot != null && characterRoot != null)
            {
                Debug.Log($"Successfully detected leg hierarchy: {upLeg.name} -> {leg.name} -> {foot.name}");
            }
            else
            {
                string missing = "";
                if (upLeg == null) missing += $"upper leg (pattern: '{upLegPattern}'), ";
                if (leg == null) missing += $"lower leg (pattern: '{legPattern}'), ";
                if (foot == null) missing += $"foot (pattern: '{footPattern}'), ";
                if (characterRoot == null) missing += "character root with Animator";
                
                Debug.LogWarning($"Could not detect complete leg hierarchy. Missing: {missing}. You can adjust the name patterns in the inspector or manually assign the bones.");
            }
        }

        #region Public Accessors
        public Transform GetTarget()
        {
            if (characterRoot == null) return null;
            
            Transform ikControls = characterRoot.Find("IK_Controls");
            if (ikControls != null)
            {
                Transform legGroup = ikControls.Find($"IK_{gameObject.name}");
                if (legGroup != null)
                {
                    return legGroup.Find($"{gameObject.name}_Target");
                }
            }
            return null;
        }

        public Transform GetHint()
        {
            if (characterRoot == null) return null;
            
            Transform ikControls = characterRoot.Find("IK_Controls");
            if (ikControls != null)
            {
                Transform legGroup = ikControls.Find($"IK_{gameObject.name}");
                if (legGroup != null)
                {
                    return legGroup.Find($"{gameObject.name}_Hint");
                }
            }
            return null;
        }

        public TwoBoneIKConstraint GetIKConstraint()
        {
            if (characterRoot == null) return null;
            
            RigBuilder rb = characterRoot.GetComponent<RigBuilder>();
            if (rb != null && rb.layers.Count > 0)
            {
                Rig rigComponent = rb.layers[0].rig;
                if (rigComponent != null)
                {
                    Transform constraints = rigComponent.transform.Find("Constraints");
                    if (constraints != null)
                    {
                        Transform constraintObj = constraints.Find($"{gameObject.name}_IKConstraint");
                        if (constraintObj != null)
                        {
                            return constraintObj.GetComponent<TwoBoneIKConstraint>();
                        }
                    }
                }
            }
            return null;
        }
        #endregion
    }
}