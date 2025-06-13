using UnityEngine;
using UnityEditor;
using System;

namespace EditorTools
{
    [InitializeOnLoad]
    public class SmartMergeRegistrator
    {
        private const string SMART_MERGE_REGISTRATOR_EDITOR_PREF_KEY = "smart_merge_installed";
        private const int VERSION = 1;

        private static readonly string VERSION_KEY = $"{VERSION}_{Application.unityVersion}";

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        static SmartMergeRegistrator()
        {
            var installedVersionKey = EditorPrefs.GetString(SMART_MERGE_REGISTRATOR_EDITOR_PREF_KEY);
            if (installedVersionKey != VERSION_KEY)
            {
                SmartMergeRegister();
            }
        }

        //---------------------------------------------------------------------
        // Menu
        //---------------------------------------------------------------------

        [MenuItem("Tools/Git/SmartMerge registration")]
        private static void SmartMergeRegister()
        {
            try
            {
                var unityYamlMergePath = $"{EditorApplication.applicationContentsPath}/Tools/UnityYAMLMerge.exe";
                ExecuteGitWithParams("config merge.unityyamlmerge.name \"Unity SmartMerge (UnityYamlMerge)\"");
                ExecuteGitWithParams($"config merge.unityyamlmerge.driver \"\\\"{unityYamlMergePath}\\\" merge -h -p --force --fallback none %O %B %A %A\"");
                ExecuteGitWithParams("config merge.unityyamlmerge.recursive binary");
                EditorPrefs.SetString(SMART_MERGE_REGISTRATOR_EDITOR_PREF_KEY, VERSION_KEY);
                Debug.Log($"Successfully registered UnityYAMLMerge with path {unityYamlMergePath}");
            }
            catch (Exception e)
            {
                Debug.Log($"Fail to register UnityYAMLMerge with error: {e}");
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void ExecuteGitWithParams(string param)
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo("git")
            {
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };


            var process = new System.Diagnostics.Process {StartInfo = processInfo};

            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = param;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception(process.StandardError.ReadLine());
            }

            process.StandardOutput.ReadLine();
        }
    }
}