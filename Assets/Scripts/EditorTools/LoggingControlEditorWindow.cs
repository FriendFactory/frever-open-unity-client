using System;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    internal sealed class LoggingControlEditorWindow: EditorWindow
    {
        [MenuItem("Tools/Friend Factory/Logging Settings")]
        public static void Display()
        {
            var window = GetWindow<LoggingControlEditorWindow>();
            window.name = "Logging Settings";
            window.minSize = new Vector2(100, 100);
            window.maxSize = new Vector2(300, 300);
        }

        private bool LoggingEnabled
        {
            get => LoggingControl.IsLoggingEnable;
            set
            {
                if(value == LoggingEnabled) return;
                LoggingControl.EnableLogging(value);
            }
        }

        private void OnGUI()
        {
            LoggingEnabled = EditorGUILayout.Toggle("Logging Enabled", LoggingEnabled);
        }
    }
}