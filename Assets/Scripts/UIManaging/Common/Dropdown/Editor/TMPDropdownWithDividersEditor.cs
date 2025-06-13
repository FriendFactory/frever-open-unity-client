using UIManaging.Core;
using UnityEditor;
using UnityEditor.UI;

namespace UIManaging.Common.Dropdown.Editor
{
    [CustomEditor(typeof(TMPDropdownWithDividers), true)]
    [CanEditMultipleObjects]
    public class TMPDropdownWithDividersEditor : DropdownEditor
    {
        SerializedProperty _divider;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _divider = serializedObject.FindProperty("_dividerPrefab");
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(_divider);
            serializedObject.ApplyModifiedProperties();
        }
    }
}