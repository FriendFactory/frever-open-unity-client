// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#if UNITY_IOS
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

namespace AdvancedInputFieldPlugin.Editor
{
	public class PostProcessBuild
	{
#if UNITY_IOS
		[PostProcessBuild]
		public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
		{
			if (buildTarget == BuildTarget.iOS)
			{
				UnityEngine.Debug.Log("Adding -force_load flag to Other Linker Flags");

				string projectPath = PBXProject.GetPBXProjectPath(path);
				PBXProject project = new PBXProject();
				project.ReadFromFile(projectPath);
				
#if UNITY_2019_3_OR_NEWER
				string target = project.GetUnityMainTargetGuid();
#else
				string target = project.TargetGuidByName(PBXProject.GetUnityTargetName()); //Use this line for older versions of Unity
#endif
                //Because this line uses hardcoded path to native plugin,
                //AdvancedInputFideld folder needs to be placed in Assets root,
                //otherwise NativeKeyboard wont be added to Xcode project.
                //Added /ExternalPackages/ to path and moved it to ExternalPackages folder
				project.AddBuildProperty(target, "OTHER_LDFLAGS", "-force_load $(PROJECT_DIR)/Libraries/ExternalPackages/AdvancedInputField/Plugins/iOS/NativeKeyboard.a");
				project.WriteToFile(projectPath);
			}
		}
#endif
	}
}

