using Common;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    //code has been copied from Cinemachine package
    [CustomPropertyDrawer(typeof(TagFieldAttribute))]
    internal sealed class TagFieldPropertyDrawer : PropertyDrawer
    {
        const float hSpace = 2;
        GUIContent clearText = new GUIContent("Clear", "Set the tag to empty");

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var textDimensions = GUI.skin.button.CalcSize(clearText);

            rect.width -= textDimensions.x + hSpace;
            string oldValue = property.stringValue;
            string newValue = EditorGUI.TagField(rect, label, oldValue);

            rect.x += rect.width + hSpace; rect.width = textDimensions.x; rect.height -=1;
            GUI.enabled = oldValue.Length > 0;
            if (GUI.Button(rect, clearText))
                newValue = string.Empty;
            GUI.enabled = true;
            if (oldValue != newValue)
                property.stringValue = newValue;
        }
    }
}
