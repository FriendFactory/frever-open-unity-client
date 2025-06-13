using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core;
using UnityEditor;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Editor
{
    [CustomEditor(typeof(CardSwipeBehaviour),true)]
    [CanEditMultipleObjects]
    public class CardEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var script = serializedObject.FindProperty("m_Script");
            var useLerp = serializedObject.FindProperty("useLerp");
            var lerp = serializedObject.FindProperty("lerp");
            var completeSwipeDuration = serializedObject.FindProperty("completeSwipeDuration");
            var completeSwipeSpeed = serializedObject.FindProperty("completeSwipeSpeed");
            var offset = serializedObject.FindProperty("offset");
            var minDragDistance = serializedObject.FindProperty("minDragDistance");
            var minDragSpeed = serializedObject.FindProperty("minDragSpeed");
            var swipeHorizontal = serializedObject.FindProperty("swipeHorizontal");
            var swipeVertical = serializedObject.FindProperty("swipeVertical");

            GUI.enabled = false;
            EditorGUILayout.PropertyField(script);
            GUI.enabled = true;
            
            EditorGUILayout.PropertyField(useLerp);
            if (useLerp.boolValue)
            {
                EditorGUILayout.PropertyField(lerp);
            }
            
            EditorGUILayout.PropertyField(completeSwipeDuration);
            EditorGUILayout.PropertyField(completeSwipeSpeed);
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.PropertyField(minDragDistance);
            EditorGUILayout.PropertyField(minDragSpeed);
            EditorGUILayout.PropertyField(swipeHorizontal);
            EditorGUILayout.PropertyField(swipeVertical);

            serializedObject.ApplyModifiedProperties();
        }
    }
}