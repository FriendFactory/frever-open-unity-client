using System;
using Bridge;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.UsersManagement;

namespace AppStart
{
    /// <summary>
    /// Responsible for passing data from Entry scene, where we should initialize or spawn only critical services and game objects
    /// </summary>
    public static class AppEntryContext
    {
        public static IBridge Bridge;
        public static UserProfileFetcher UserProfileFetcher;
        public static EntryState State;
        public static VideoPlayerCanvas VideoPlayerCanvas;
        public static CriticalDataFetcher CriticalDataFetcher;
        public static LocalizationSetup LocalizationSetup;
        public static Action OnLoadingStarted;
        public static Action OnLoadingDone;
    }

    public enum EntryState
    {
        NotLoggedIn,
        LoggedIn,
        NoInternetConnection,
        OutdatedApp
    }
}