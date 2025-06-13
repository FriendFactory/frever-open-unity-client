using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    [InitializeOnLoad]
    public class GitHooksRegistrator
    {
        private const int PRE_COMMIT_VERSION = 1;

        private const string PRE_COMMIT_PATH = ".git/hooks/pre-commit";
        private const string PRE_COMMIT_BAK_PATH = PRE_COMMIT_PATH + ".bak";
        private const string PRE_COMMIT_TEMPLATE_PATH = "Assets/Scripts/EditorTools/Git/Hooks/pre-commit";

        private static readonly string PRE_COMMIT_PREF_KEY;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        static GitHooksRegistrator()
        {
            PRE_COMMIT_PREF_KEY = $"{Base64Encode(Application.dataPath)}.git_hook_pre_commit";
            
            var installedVersionKey = EditorPrefs.GetInt(PRE_COMMIT_PREF_KEY, 0);
            if (installedVersionKey < PRE_COMMIT_VERSION)
            {
                RegisterPreCommitHook();
            }
        }

        //---------------------------------------------------------------------
        // Menu
        //---------------------------------------------------------------------

        [MenuItem("Tools/Git/Pre-commit/Register hook")]
        private static void RegisterPreCommitHook()
        {
            try
            {
                if (!File.Exists(PRE_COMMIT_TEMPLATE_PATH))
                {
                    Debug.LogError("[DEV] Pre-commit template not found.");
                    return;
                }
                
                if (File.Exists(PRE_COMMIT_PATH))
                {
                    FileUtil.ReplaceFile(PRE_COMMIT_PATH, PRE_COMMIT_BAK_PATH);
                    Debug.Log("[DEV] Previous pre-commit hook found. Backup created.");
                }
                
                FileUtil.ReplaceFile(PRE_COMMIT_TEMPLATE_PATH, PRE_COMMIT_PATH);
                EditorPrefs.SetInt(PRE_COMMIT_PREF_KEY, PRE_COMMIT_VERSION);
                
                Debug.Log($"[DEV] Successfully registered git pre-commit hook");
            }
            catch (Exception e)
            {
                Debug.Log($"[DEV] Fail to register git pre-commit hook with error: {e}");
            }
        }
        
        [MenuItem("Tools/Git/Pre-commit/Disable till next run")]
        private static void DisablePreCommitHook()
        {
            try
            {
                FileUtil.DeleteFileOrDirectory(PRE_COMMIT_PATH);
                EditorPrefs.SetInt(PRE_COMMIT_PREF_KEY, 0);
                
                Debug.Log($"[DEV] Disabled git pre-commit hook until next run");
            }
            catch (Exception e)
            {
                Debug.Log($"[DEV] Fail to disable git pre-commit hook with error: {e}");
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static string Base64Encode(string plainText) {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}