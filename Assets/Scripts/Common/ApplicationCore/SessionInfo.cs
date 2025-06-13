using Bridge;
using UnityEngine;
using Zenject;

namespace Common.ApplicationCore
{
    public interface ISessionInfo
    {
        bool PreviousSessionCrashed { get; }
        FFEnvironment Environment { get; }
    }

    /// <summary>
    /// Stores in Player prefs key during app session running and removes it on application proper closing
    /// If key is persisted on next session startup, then previous session was crashed
    /// </summary>
    internal sealed class SessionInfo : MonoBehaviour, ISessionInfo
    {
        private const string SESSION_RUNNING_KEY = "session_is_running";
        private IEnvironmentInfo _environmentInfo;
        private bool? _previousSessionWasCrashed;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private bool HasKey => PlayerPrefs.HasKey(SESSION_RUNNING_KEY);

        public bool PreviousSessionCrashed
        {
            get{
                if (!_previousSessionWasCrashed.HasValue)
                {
                    _previousSessionWasCrashed = HasKey;
                }
                return _previousSessionWasCrashed.Value;
            }
            private set => _previousSessionWasCrashed = value;
        }

        public FFEnvironment Environment => _environmentInfo.Environment;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        private void Construct(IEnvironmentInfo environmentInfo)
        {
            _environmentInfo = environmentInfo;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            PreviousSessionCrashed = HasKey;
            AddKey();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                AddKey();
            }
            else
            {
                RemoveKey();
            }
        }
        
        private void OnApplicationQuit()
        {
            RemoveKey();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void AddKey()
        {
            PlayerPrefs.SetInt(SESSION_RUNNING_KEY, 0);
            PlayerPrefs.Save();
        }

        private void RemoveKey()
        {
            PlayerPrefs.DeleteKey(SESSION_RUNNING_KEY);
            PlayerPrefs.Save();
        }
    }
}