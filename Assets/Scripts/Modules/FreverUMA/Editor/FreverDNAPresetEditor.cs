using UnityEngine;
using UnityEditor;
using UMA;

namespace Modules.FreverUMA {
    [CustomEditor(typeof(FreverDNAPreset))]
    public class FreverDNAPresetEditor : Editor
    {
        private SerializedProperty _dnaProperty;
        private SerializedProperty _presetsProperty;
        private SerializedProperty _raceDependentProperty;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable() {
            _dnaProperty = serializedObject.FindProperty("PresetType");
            _presetsProperty = serializedObject.FindProperty("Presets");
            _raceDependentProperty = serializedObject.FindProperty("RaceDependent");
        }

        public override void OnInspectorGUI() {
            var thumb = serializedObject.FindProperty("Thumbnail");
			thumb.objectReferenceValue = EditorGUILayout.ObjectField("Thumbnail", thumb.objectReferenceValue, typeof(Sprite), false);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_dnaProperty, new GUIContent("DNA Preset"));
            _raceDependentProperty.boolValue = EditorGUILayout.Toggle("Race Dependent", _raceDependentProperty.boolValue);

            var presetType = (DynamicUMADnaAsset) _dnaProperty.objectReferenceValue;

            if(EditorGUI.EndChangeCheck()) {
                if(presetType != null) {
                    _presetsProperty.ClearArray();
                    for(var i = 0; i < 2; i++) {
                        foreach (var presetName in presetType.Names) {
                            _presetsProperty.arraySize ++;
                            var entry = _presetsProperty.GetArrayElementAtIndex(_presetsProperty.arraySize - 1);
                            entry.FindPropertyRelative("Name").stringValue = presetName;
                            entry.FindPropertyRelative("Value").floatValue = 0.5f;
                        }
                    }
                }
            }

            if(!_raceDependentProperty.boolValue) {
                for(var i = 0; i < _presetsProperty.arraySize / 2; i++) {
                    var entry = _presetsProperty.GetArrayElementAtIndex(i);
                    entry.FindPropertyRelative("Value").floatValue = EditorGUILayout.Slider(entry.FindPropertyRelative("Name").stringValue, entry.FindPropertyRelative("Value").floatValue, 0f, 1f);
                }
            }

            else {
                var index = 0;

                EditorGUILayout.LabelField("Male");
                for(var i = 0; i < _presetsProperty.arraySize / 2; i++) {
                    var entry = _presetsProperty.GetArrayElementAtIndex(index);
                    entry.FindPropertyRelative("Value").floatValue = EditorGUILayout.Slider(entry.FindPropertyRelative("Name").stringValue, entry.FindPropertyRelative("Value").floatValue, 0f, 1f);
                    index ++;
                }
                EditorGUILayout.LabelField("Female");
                for(var i = 0; i < _presetsProperty.arraySize / 2; i++) {
                    var entry = _presetsProperty.GetArrayElementAtIndex(index);
                    entry.FindPropertyRelative("Value").floatValue = EditorGUILayout.Slider(entry.FindPropertyRelative("Name").stringValue, entry.FindPropertyRelative("Value").floatValue, 0f, 1f);
                    index ++;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
