using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Common
{
    [CreateAssetMenu(fileName = nameof(RuntimeBuildInfo), menuName = nameof(RuntimeBuildInfo), order = 1)]
    public class RuntimeBuildInfo : ScriptableObject
    {
        [SerializeField, ReadOnly] private string _buildNumber;
        public string BuildNumber 
        {
            get
            {
                var buildNumber = string.Empty;
                
#if UNITY_EDITOR
                buildNumber = PlayerSettings.iOS.buildNumber;
#else
                buildNumber = _buildNumber;
#endif
                return buildNumber;
            }
        }

        public void SetBuildNumber(string buildNumber)
        {
            _buildNumber = buildNumber;
        }
    }
}

