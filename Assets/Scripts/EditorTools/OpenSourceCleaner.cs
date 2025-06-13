using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class OpenSourceCleaner : EditorWindow
{
    private OpenSourceCleanupConfig config;

    [MenuItem("Tools/Open Source/Cleanup Tool")]
    public static void ShowWindow()
    {
        GetWindow<OpenSourceCleaner>("Open Source Cleaner");
    }

    private void OnGUI()
    {
        GUILayout.Label("🧼 Open Source Cleanup", EditorStyles.boldLabel);
        config = (OpenSourceCleanupConfig)EditorGUILayout.ObjectField("Cleanup Config", config, typeof(OpenSourceCleanupConfig), false);

        if (config != null)
        {
            if (GUILayout.Button("🧹 Run Cleanup"))
            {
                RunCleanup(config);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Оберіть конфігураційний файл ScriptableObject.", MessageType.Info);
        }
    }

    public static void RunCleanup(OpenSourceCleanupConfig config)
    {
        Debug.Log("💡 Cleanup started...");

        // 1. Delete selected folders
        foreach (var obj in config.foldersToDelete)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(assetPath))
            {
                Debug.Log($"🧹 Deleting folder: {assetPath}");
                FileUtil.DeleteFileOrDirectory(assetPath);
                FileUtil.DeleteFileOrDirectory(assetPath + ".meta");
            }
            else
            {
                Debug.LogWarning($"⚠️ Skipping non-folder or missing path: {assetPath}");
            }
        }

        AssetDatabase.Refresh();

        // 2. Clean manifest.json
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        if (!File.Exists(manifestPath))
        {
            Debug.LogError("❌ manifest.json not found!");
            return;
        }

        string json = File.ReadAllText(manifestPath);
        var lines = json.Split('\n').ToList();

        // Backup
        File.WriteAllText(manifestPath + ".bak", json);
        Debug.Log("📦 Backup created: manifest.json.bak");

        lines = lines.Where(line =>
            !config.packagesToDelete.Any(pkg => line.Contains($"\"{pkg}\""))
        ).ToList();

        File.WriteAllText(manifestPath, string.Join("\n", lines));
        Debug.Log("✅ manifest.json cleaned");

        AssetDatabase.Refresh();
        Debug.Log("✅ Cleanup complete.");
    }
}
