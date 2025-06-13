using System.Collections.Generic;
using System.Linq;
using Common;
using Extensions;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Modules.LevelManaging.Editing.CameraManaging.CameraSpawnFormation
{
    [CustomEditor(typeof(SpawnFormationCameraSetup))]
    internal sealed class SpawnFormationCameraSetupEditor : OdinEditor
    {
        private SpawnFormationCameraSetup _target;

        private List<SpawnFormationCameraData> FormationSetups
        {
            get => _target.FormationSetups;
            set => _target.FormationSetups = value;
        } 
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _target = (SpawnFormationCameraSetup)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                Validate();
            }
        }

        private void Validate()
        {
            if (FormationSetups == null) FormationSetups = new List<SpawnFormationCameraData>();

            foreach (var setup in FormationSetups)
            {
                ValidateCameraAngleDataCount(setup);
                ValidateCharacterSequenceNumbers(setup);
            }
        }

        private void ValidateCharacterSequenceNumbers(SpawnFormationCameraData setup)
        {
            if (ContainsDifferentSettingsForSameCharacter(setup))
            {
                DisplayError("Spawn Formation Setup Error",
                    $"{setup.FormationType} camera formation setup contains duplicated target character sequence numbers",
                    "Got It");
            }
            
            if (ContainsNonValidSequenceNumbers(setup))
            {
                DisplayError("Spawn Formation Setup Error", $"{setup.FormationType} camera formation setup contains character sequence number higher than max value " +
                                                            $"(max: {Constants.CHARACTERS_IN_SPAWN_FORMATION_MAX}). Allowed values from 0 till {Constants.CHARACTERS_IN_SPAWN_FORMATION_MAX - 1}", "Got It"); ;
            }
        }

        private static bool ContainsNonValidSequenceNumbers(SpawnFormationCameraData setup)
        {
            return setup.CameraSettings.Any(_ => _.CharacterSequenceNumber >= Constants.CHARACTERS_IN_SPAWN_FORMATION_MAX);
        }

        private static bool ContainsDifferentSettingsForSameCharacter(SpawnFormationCameraData setup)
        {
            return setup.CameraSettings.GroupBy(x=>x.CharacterSequenceNumber).Any(x=>x.Count()>1);
        }

        private void ValidateCameraAngleDataCount(SpawnFormationCameraData setup)
        {
            if (setup.CameraSettings.IsNullOrEmpty())
            {
                //warning instead of DisplayDialog, because OnValidate triggers just after you add one more Formation Setup into list
                Debug.LogWarning(($"{setup.FormationType} camera formation setup has empty list of target character sequence numbers"));
            }
        }

        private void DisplayError(string title, string message, string ok)
        {
            EditorApplication.Beep();
            EditorUtility.DisplayDialog(title, message, ok);
        }
    }
}
