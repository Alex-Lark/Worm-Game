using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EffectGroup.Effect))]
public class EffectDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float lineHeight = EditorGUIUtility.singleLineHeight + 2;

        // Draw _type enum
        SerializedProperty typeProp = property.FindPropertyRelative("_type");
        EditorGUI.PropertyField(
            new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
            typeProp
        );
        y += lineHeight;

        EffectGroup.Effect.EffectType type = (EffectGroup.Effect.EffectType)typeProp.enumValueIndex;

        // Show relevant field based on type
        switch (type)
        {
            case EffectGroup.Effect.EffectType.ScreenShake:
                DrawField(property, "_intensity", position.x, ref y);
                break;
            case EffectGroup.Effect.EffectType.SoundEffect:
                DrawField(property, "_audioContainer", position.x, ref y);
                break;
            case EffectGroup.Effect.EffectType.PooledEffect:
                DrawField(property, "_pooledEffectPrefab", position.x, ref y);
                break;
        }

        EditorGUI.EndProperty();
    }

    private void DrawField(SerializedProperty parent, string fieldName, float x, ref float y)
    {
        SerializedProperty prop = parent.FindPropertyRelative(fieldName);
        if (prop != null)
        {
            EditorGUI.PropertyField(
                new Rect(x, y, EditorGUIUtility.currentViewWidth - 40, EditorGUIUtility.singleLineHeight),
                prop
            );
            y += EditorGUIUtility.singleLineHeight + 2;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + 2; // for _type

        EffectGroup.Effect.EffectType type = (EffectGroup.Effect.EffectType)property.FindPropertyRelative("_type").enumValueIndex;

        switch (type)
        {
            case EffectGroup.Effect.EffectType.ScreenShake:
                height += EditorGUIUtility.singleLineHeight + 2; // _intensity
                break;
            case EffectGroup.Effect.EffectType.SoundEffect:
                height += EditorGUIUtility.singleLineHeight + 2; // _audioContainer
                break;
            case EffectGroup.Effect.EffectType.PooledEffect:
                height += EditorGUIUtility.singleLineHeight + 2; // _pooledEffectPrefab
                break;
        }

        return height;
    }
}