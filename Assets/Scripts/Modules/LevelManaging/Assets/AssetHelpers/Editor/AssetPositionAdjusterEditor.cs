using UnityEditor;
using UnityEngine;
using Space = Bridge.Models.ClientServer.Assets.Space;

namespace Modules.LevelManaging.Assets.AssetHelpers
{
    [CustomEditor(typeof(VfxPositionAdjuster))]
    public sealed class AssetPositionAdjusterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            VfxPositionAdjuster adjuster = (VfxPositionAdjuster)target;

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Custom Controls", EditorStyles.boldLabel);

            adjuster.TrackRotation = EditorGUILayout.Toggle("Track Rotation", adjuster.TrackRotation);
            
            adjuster.PositionSpace = (Space?)EditorGUILayout.EnumPopup("Position Space", adjuster.PositionSpace ?? Space.Local);
            
            adjuster.AdjustPosition = EditorGUILayout.Vector3Field("Adjust Position", adjuster.AdjustPosition ?? Vector3.zero);

            adjuster.AdjustRotation = EditorGUILayout.Vector3Field("Adjust Rotation", adjuster.AdjustRotation ?? Vector3.zero);
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(adjuster);
            }
        }
    }
}