using System.Timers;
using Bridge;
using UIManaging.Pages.Common.UserLoginManagement;
using UnityEngine;
using Zenject;

namespace Modules
{
    public class OnlinePingService : MonoBehaviour
    {
        private Timer _timer;

        [SerializeField] private int _pingIntervalSeconds = 300;

        [Inject] private IBridge _bridge;
        [Inject] private UserAccountManager _userAccountManager;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _timer = new Timer(_pingIntervalSeconds * 1000);
            _timer.Elapsed += PingOnline;
            
            if (_userAccountManager.IsLoggedIn)
            {
                OnUserLoggedIn();
                return;
            }

            _userAccountManager.OnUserLoggedIn += OnUserLoggedIn;
        }

        private void OnUserLoggedIn()
        {
            _userAccountManager.OnUserLoggedIn -= OnUserLoggedIn;
            _userAccountManager.OnUserLoggedOut += OnUserLoggedOut;
            StartTimer();
        }

        private void OnUserLoggedOut()
        {
            _userAccountManager.OnUserLoggedOut -= OnUserLoggedOut;
            _userAccountManager.OnUserLoggedIn += OnUserLoggedIn;
            StopTimer();
        }

        private void StartTimer()
        {
            PingOnline(this, null);
            _timer.Start();
        }
        
        private void StopTimer()
        {
            _timer.Stop();
        }

        private void PingOnline(object sender, ElapsedEventArgs e)
        {
            _bridge.UpdateOnlineStatus();
        }

        private void OnDestroy()
        {
            if(_timer == null) return;
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!_userAccountManager.IsLoggedIn) return;
            
            if (pauseStatus)
            {
                StopTimer();
            }
            else
            {
                StartTimer();
            }
        }
    }
}