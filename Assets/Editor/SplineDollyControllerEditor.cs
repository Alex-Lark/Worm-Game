using Graphics;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(SplineDollyController))]
    public class SplineDollyControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            SplineDollyController controller = (SplineDollyController)target;

            controller.playbackSpeed = EditorGUILayout.FloatField("Speed", controller.playbackSpeed);
            controller.autoPlayOnStart = EditorGUILayout.Toggle("Auto Play On Start", controller.autoPlayOnStart);
            controller.endBehaviour = (EndBehaviour)EditorGUILayout.EnumPopup("End Behaviour", controller.endBehaviour);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dolly Playback", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = controller.isPlaying ? Color.green : Color.white;
            if (GUILayout.Button("▶  Play"))
            {
                controller.Play();
                EditorApplication.update += ForceRepaint;
            }

            GUI.backgroundColor = !controller.isPlaying ? Color.yellow : Color.white;
            if (GUILayout.Button("⏹  Stop"))
            {
                controller.Stop();
                EditorApplication.update -= ForceRepaint;
            }

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("↺  Reset"))
            {
                controller.Reset();
                EditorApplication.update -= ForceRepaint;
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (controller.isPlaying)
                EditorUtility.SetDirty(controller);
        }

        private void ForceRepaint()
        {
            SplineDollyController controller = (SplineDollyController)target;
            if (controller != null && controller.isPlaying)
            {
                controller.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
                SceneView.RepaintAll();
            }
        }
    }
}