using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Navigation.Args;
using Navigation.Core;
using QFSW.QC;
using UIManaging.Pages.Common.UserLoginManagement;
using UIManaging.Pages.Common.UsersManagement;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UnityEngine;
using Zenject;

namespace Development.Console.User
{
    public class ConsoleSessionManager
    {
        private const string SAVED_SESSIONS_FOLDER  = "SavedSessions";
        private static string _savedSessionsPath;

        private static IBridge _bridge;
        private static LocalUserDataHolder _localUser;
        private static PageManager _pageManager;
        private static UserAccountManager _userAccountManager;
        
        private static IList<string> SaveFiles => Directory.GetFiles(_savedSessionsPath, "*.ses").OrderBy(File.GetCreationTimeUtc).ToList();
        
        static ConsoleSessionManager()
        {
            _bridge = ProjectContext.Instance.Container.Resolve<IBridge>();
            _localUser = ProjectContext.Instance.Container.Resolve<LocalUserDataHolder>();
            _pageManager = ProjectContext.Instance.Container.Resolve<PageManager>();
            _userAccountManager = ProjectContext.Instance.Container.Resolve<UserAccountManager>();
            
            _savedSessionsPath =  Path.Combine(Application.persistentDataPath, SAVED_SESSIONS_FOLDER);

            if (!Directory.Exists(_savedSessionsPath)) Directory.CreateDirectory(_savedSessionsPath);
        }

        [Command("u-save")]
        [Command("user-save")]
        private static void SaveSession()
        {
            File.Copy(Path.Combine(Application.persistentDataPath, "user_data"),
                Path.Combine(_savedSessionsPath, $"{_localUser.NickName} {_localUser.GroupId} {_bridge.Environment}.ses"), true);
            
            Debug.Log("User session saved");
        }

        [Command("u-del")] 
        [Command("user-delete")]
        private static void DeleteSession(int index)
        {
            var saveFile = SaveFiles[index - 1];
            File.Delete(saveFile);
            Debug.Log($"User session {Path.GetFileName(saveFile)} deleted");
        }

        [Command("u")]
        [Command("user")]
        private static void ListStoredSessions()
        {
            Debug.Log(_bridge.Profile == null
                          ? "No active session"
                          : $"Current user: {_localUser.NickName} {_localUser.GroupId} {_bridge.Environment}");

            var saveFiles = SaveFiles;
            Debug.Log($"Stored user sessions: {saveFiles.Count}");

            for (var i = 0; i < saveFiles.Count; i++)
            {
                var file = saveFiles[i];
                Debug.Log($"{i+1} {Path.GetFileName(file)}");
            }
        }
        
        [Command("u")]
        [Command("user")]
        public static void LoadSessionByFileIndex(int index)
        {
            _userAccountManager.Logout(OnComplete, OnFail);
            
            async void OnComplete()
            {
                var saveFile = SaveFiles[index - 1];
                Debug.Log($"Loading user session: {Path.GetFileName(saveFile)}");
                File.Copy(saveFile, Path.Combine(Application.persistentDataPath, "user_data"), true);
                var loginResult = await _bridge.ReloadLastSavedUserAndLogin();
                if (loginResult.IsSuccess)
                {
                    Debug.Log($"Log in success");
                    _pageManager.MoveNext(new UserProfileArgs());
                }
                else
                {
                    Debug.LogError($"Log in failed:\n{loginResult.ErrorMessage}");
                    _pageManager.MoveNext(new OnBoardingPageArgs());
                }
            }
            
            void OnFail(string error)
            {
                Debug.LogError($"Log out failed:\n{error}");
            }
        }
    }
}