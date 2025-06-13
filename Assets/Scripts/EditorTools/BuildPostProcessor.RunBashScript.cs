using System.Diagnostics;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace EditorTools
{
    public static partial class BuildPostProcessor
    {
        [PostProcessBuild(60)]
        public static void FixObsoleteCodeInBuild(BuildTarget target, string path)
        {
            #if UNITY_IOS
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/bin/bash";
            startInfo.Arguments = $"{Application.dataPath}/Editor/post_process_bash.sh {path}";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true; // Redirect script output to Unity console

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            // Read the output of the script
            string output = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log(output);

            process.WaitForExit();
            process.Close();
            
            #endif
        }
    }
}