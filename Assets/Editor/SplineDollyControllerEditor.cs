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
            DrawDefaultInspector();

            SplineDollyController controller = (SplineDollyController)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dolly Playback", EditorStyles.boldLabel);

            // Playback speed field
            controller.playbackSpeed = EditorGUILayout.FloatField("Speed", controller.playbackSpeed);

            EditorGUILayout.BeginHorizontal();

            // Play button (green tint when active)
            GUI.backgroundColor = controller.isPlaying ? Color.green : Color.white;
            if (GUILayout.Button("▶  Play"))
            {
                controller.Play();
                // Drive Update() in edit mode
                EditorApplication.update += ForceRepaint;
            }

            // Stop button
            GUI.backgroundColor = !controller.isPlaying ? Color.yellow : Color.white;
            if (GUILayout.Button("⏹  Stop"))
            {
                controller.Stop();
                EditorApplication.update -= ForceRepaint;
            }

            // Reset button
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("↺  Reset"))
            {
                controller.Reset();
                EditorApplication.update -= ForceRepaint;
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            // Mark dirty so the scene updates
            if (controller.isPlaying)
                EditorUtility.SetDirty(controller);
        }

        private void ForceRepaint()
        {
            // Tick the controller manually in edit mode
            SplineDollyController controller = (SplineDollyController)target;
            if (controller != null && controller.isPlaying)
            {
                controller.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
                SceneView.RepaintAll();
            }
        }
    }
}