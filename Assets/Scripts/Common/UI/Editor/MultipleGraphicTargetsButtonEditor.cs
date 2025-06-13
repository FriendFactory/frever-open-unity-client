#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UI;

namespace Common.UI.Editor
{
    [CustomEditor(typeof(MultipleGraphicTargetsButton), true)]
    [CanEditMultipleObjects]
    public sealed class MultipleGraphicTargetsButtonEditor : ButtonEditor
    {
        private SerializedProperty _targetGraphicsProperty;

        protected override void OnEnable()
        {
            _targetGraphicsProperty = serializedObject.FindProperty("_graphics");
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_targetGraphicsProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif