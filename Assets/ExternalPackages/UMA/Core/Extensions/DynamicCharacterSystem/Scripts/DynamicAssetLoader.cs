using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UMA.AssetBundles;

namespace UMA.CharacterSystem
{
    public class DynamicAssetLoader : MonoBehaviour
    {
        static DynamicAssetLoader _instance;

        public bool makePersistent;
        [Space]
        [Tooltip("Set the server URL that assetbundles can be loaded from. Used in a live build and when the LocalAssetServer is turned off. Requires trailing slash but NO platform name")]
        public string remoteServerURL = "";
        [Tooltip("Use the JSON version of the assetBundleIndex rather than the assetBundleVersion.")]
        public bool useJsonIndex = false;
        [Tooltip("Set the server URL for the AssetBundleIndex json data. You can use this to make a server request that could generate an index on the fly for example. Used in a live build and when the LocalAssetServer is turned off. TIP use [PLATFORM] to use the current platform name in the URL")]
        public string remoteServerIndexURL = "";
        [Space]
        //EncryptedAssetBundles
        public bool useEncryptedBundles = false;
        public string bundleEncryptionPassword = "";
        [Space]
        [Tooltip("A list of assetbundles to preload when the game starts. After these have completed loading any GameObject in the gameObjectsToActivate field will be activated.")]
        public List<string> assetBundlesToPreLoad = new List<string>();
        [Tooltip("GameObjects that will be activated after the list of assetBundlesToPreLoad has finished downloading.")]
        public List<GameObject> gameObjectsToActivate = new List<GameObject>();
        [Tooltip("GameObjects that will be activated after Initialization completes.")]
        public List<GameObject> gameObjectsToActivateOnInit = new List<GameObject>();
        [Space]
        public GameObject loadingMessageObject;
        public Text loadingMessageText;
        public string loadingMessage = "";
        [HideInInspector]
        [System.NonSerialized]
        public float percentDone = 0f;
        [HideInInspector]
        [System.NonSerialized]
        public bool assetBundlesDownloading;
        [HideInInspector]
        [System.NonSerialized]
        public bool canCheckDownloadingBundles;
        protected bool isInitializing = false;
        [HideInInspector]
        public bool isInitialized = false;
        [Space]
        [System.NonSerialized]
        [ReadOnly]
        public DownloadingAssetsList downloadingAssets = new DownloadingAssetsList();

        public Action<string> BundleLoaded;

        //For SimulationMode in the editor - equivalent of AssetBundleManager.m_downloadedBundles
        //should persist betweem scene loads but not between plays
#if UNITY_EDITOR
        List<string> simulatedDownloadedBundles = new List<string>();
#endif

        public static DynamicAssetLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindInstance();
                }
                return _instance;
            }
            set { _instance = value; }
        }

        #region BASE METHODS
        /*void OnEnable()
        {
            if (_instance == null) _instance = this;
            if (!isInitialized)
            {
                StartCoroutine(Initialize());
            }
        }*/
        protected void Awake()
        {
            if (!isInitialized && Application.isPlaying)
                StartCoroutine(StartCO());
        }

        void Start()
        {
            if (!isInitializing && !isInitialized)
                StartCoroutine(StartCO());
        }

		//New method to trigger Initialization again if it fails because there is no Internet Connection
		public void ReInitialize()
		{
			if (!isInitializing && !isInitialized)
			{
				StartCoroutine(StartCO());
			}
		}

        IEnumerator StartCO()
        {
            var destroyingThis = false;
            if (_instance == null)
            {
                _instance = this;
                if (makePersistent)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
                if (!isInitialized)
                {
                    yield return StartCoroutine(Initialize());
                }
            }
            else if (_instance != this)
            {
                //copy some values over and then destroy this
                if (_instance.makePersistent)
                {
                    if (Debug.isDebugBuild)
                        Debug.Log("[DynamicAssetLoader] _instance was NOT this one and was persistent");
                    _instance.assetBundlesToPreLoad.Clear();
                    _instance.assetBundlesToPreLoad.AddRange(this.assetBundlesToPreLoad);
                    _instance.gameObjectsToActivate.Clear();
                    _instance.gameObjectsToActivate.AddRange(this.gameObjectsToActivate);
                    _instance.remoteServerIndexURL = this.remoteServerIndexURL;
                    UMAUtils.DestroySceneObject(this.gameObject);
                    destroyingThis = true;
                }
                else
                {
                    _instance = this;
                }
            }
            else if (_instance == this)//sometimes things have called Instance before Start has actually happenned on this
            {
                if (makePersistent)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
                if (!isInitialized)
                {
                    yield return StartCoroutine(Initialize());
                }
            }

            //Load any preload asset bundles if there are any
			//If we are not initialized dont to this
            if (!destroyingThis && isInitialized)
                if (assetBundlesToPreLoad.Count > 0)
                {
                    var bundlesToSend = new List<string>(assetBundlesToPreLoad.Count);
                    bundlesToSend.AddRange(assetBundlesToPreLoad);
                    StartCoroutine(LoadAssetBundlesAsync(bundlesToSend));
                    assetBundlesToPreLoad.Clear();
                }
        }

        void Update()
        {
			//If we are not initialized dont to this
			if (assetBundlesToPreLoad.Count > 0 && isInitialized)
            {
                var bundlesToSend = new List<string>(assetBundlesToPreLoad.Count);
                bundlesToSend.AddRange(assetBundlesToPreLoad);
                StartCoroutine(LoadAssetBundlesAsync(bundlesToSend));
                assetBundlesToPreLoad.Clear();
            }
#if UNITY_EDITOR
            if (AssetBundleManager.SimulateAssetBundleInEditor)
            {
                if (gameObjectsToActivate.Count > 0)
                {
                    foreach (var go in gameObjectsToActivate)
                    {
                        if (!go.activeSelf)
                        {
                            go.SetActive(true);
                        }
                    }
                    gameObjectsToActivate.Clear();
                }
            }
            else
            {
#endif
                if (downloadingAssets.downloadingItems.Count > 0)
                    downloadingAssets.Update();
                if (downloadingAssets.areDownloadedItemsReady == false)
                    assetBundlesDownloading = true;
                if ((assetBundlesDownloading || downloadingAssets.areDownloadedItemsReady == false) && canCheckDownloadingBundles == true)
                {
                    if (!AssetBundleManager.AreBundlesDownloading() && downloadingAssets.areDownloadedItemsReady == true)
                    {
                        assetBundlesDownloading = false;
                        if (gameObjectsToActivate.Count > 0)
                        {
                            foreach (var go in gameObjectsToActivate)
                            {
                                if (!go.activeSelf)
                                {
                                    go.SetActive(true);
                                }
                            }
                            gameObjectsToActivate.Clear();
                        }
                    }
                }
#if UNITY_EDITOR
            }
#endif
        }

        /// <summary>
        /// Finds the DynamicAssetLoader in the scene and treats it like a singleton.
        /// </summary>
        /// <returns>The DynamicAssetLoader.</returns>
        public static DynamicAssetLoader FindInstance()
        {
            if (_instance == null)
            {
                var dynamicAssetLoaders = FindObjectsOfType(typeof(DynamicAssetLoader)) as DynamicAssetLoader[];
                if (dynamicAssetLoaders.Length > 0)
                {
                    _instance = dynamicAssetLoaders[0];
                }
            }
            return _instance;
        }
        #endregion

        #region CHECK DOWNLOADS METHODS
        public bool downloadingAssetsContains(string assetToCheck)
        {
            return downloadingAssets.DownloadingItemsContains(assetToCheck);
        }

        public bool downloadingAssetsContains(List<string> assetsToCheck)
        {
            return downloadingAssets.DownloadingItemsContains(assetsToCheck);
        }

        #endregion

        #region DOWNLOAD METHODS

        /// <summary>
        /// Initialize the downloading URL. eg. local server / iOS ODR / or the download URL as defined in the component settings if Simulation Mode and Local Asset Server is off
        /// </summary>
        protected void InitializeSourceURL()
        {
            var URLToUse = "";
            if (SimpleWebServer.ServerURL != "")
            {
#if UNITY_EDITOR
                if (SimpleWebServer.serverStarted)//this is not true in builds no matter what- but we in the editor we need to know
#endif
                    URLToUse = remoteServerURL = SimpleWebServer.ServerURL;
                if (Debug.isDebugBuild)
                    Debug.Log("[DynamicAssetLoader] SimpleWebServer.ServerURL = " + URLToUse);
            }
            else
            {
                URLToUse = remoteServerURL;
            }
            if (URLToUse != "")
                AssetBundleManager.SetSourceAssetBundleURL(URLToUse);
            else
            {
                var errorString = "LocalAssetBundleServer was off and no remoteServerURL was specified. One of these must be set in order to use any AssetBundles!";
                var warningType = "warning";
#if UNITY_EDITOR
                errorString = "Switched to Simulation Mode because LocalAssetBundleServer was off and no remoteServerURL was specified in the Scenes' DynamicAssetLoader. One of these must be set in order to actually use your AssetBundles.";
                warningType = "info";
#endif
                //AssetBundleManager.SimulateOverride = true; // Fenrir 04.03.2020 / if we set flag to false - we need it
                var context = UMAContext.FindInstance();
                if (context != null)
                {
                    if ((context.dynamicCharacterSystem != null && (context.dynamicCharacterSystem as DynamicCharacterSystem).dynamicallyAddFromAssetBundles)
                        || (context.raceLibrary != null && (context.raceLibrary as DynamicRaceLibrary).dynamicallyAddFromAssetBundles)
                        || (context.slotLibrary != null && (context.slotLibrary as DynamicSlotLibrary).dynamicallyAddFromAssetBundles)
                        || (context.overlayLibrary != null && (context.overlayLibrary as DynamicOverlayLibrary).dynamicallyAddFromAssetBundles))
                    {
                        if (warningType == "warning")
                        {
                            if (Debug.isDebugBuild)
                                Debug.LogWarning(errorString);
                        }
                        else
                        {
                            if (Debug.isDebugBuild)
                                Debug.Log(errorString);
                        }
                    }
                }
                else //if you are just using dynamicassetLoader independently of UMA then you may still want this message
                {
                    if (warningType == "warning")
                    {
                        if (Debug.isDebugBuild)
                            Debug.LogWarning(errorString);
                    }
                    else
                    {
                        if (Debug.isDebugBuild)
                            Debug.Log(errorString);
                    }
                }
            }
            return;

        }
        /// <summary>
        /// Initializes AssetBundleManager which loads the AssetBundleManifest object and the AssetBundleIndex object.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator Initialize()
        {
#if UNITY_EDITOR
            if (AssetBundleManager.SimulateAssetBundleInEditor)
            {
                isInitialized = true;
                yield break;
            }
#endif
            if (isInitializing == false)
            {
                isInitializing = true;
                //If we are using encryption set the encryption key in AssetBundleManager
                if (useEncryptedBundles)
                {
                    if (bundleEncryptionPassword != "")
                    {
#if UNITY_EDITOR
                        //if the set password and the UMAAssetBundleManagerSettings Passwords dont match and we are in the editor warn the user
                        if (bundleEncryptionPassword != UMAABMSettings.GetEncryptionPassword())
                        {
                            if (Debug.isDebugBuild)
                                Debug.LogWarning("The bundle encryption password set in this scenes DynamicAssetLoader did not match the one set in UMAAssetBundleManagerSettings. You can fix this by inspecting the DynamicAssetLoader component. It will update automatically.");
                        }
#endif
                        AssetBundleManager.BundleEncryptionKey = bundleEncryptionPassword;
                    }
#if UNITY_EDITOR
                    else
                    {
                        //if an encryption key has been generated but it has not been assigned to this DAL show a warning that it needs to be assigned but assign it anyway
                        if (UMAABMSettings.GetEncryptionPassword() != "")
                        {
                            AssetBundleManager.BundleEncryptionKey = UMAABMSettings.GetEncryptionPassword();
                            if (Debug.isDebugBuild)
                                Debug.LogWarning("You are using encrypted asset bundles but you have not assigned the encryption key to this scenes DynamicAssetLoader, you need to do this before you build your game or your bundles will not be decrypted!");
                        }
                        //if an encryption key has NOT been generated show a warnining that the user need to generate one in the UMAAssetBundleManager window
                        else
                        {
                            if (Debug.isDebugBuild)
                                Debug.LogWarning("The DynamicAssetLoader in this scene is set to use encrypted bundles but you have not generated an encryption key yet. Please go to UMAAssetBundleManager to generate one, then assign it to your DynamicAssetLoader components in your scene.");
                        }
                    }
#endif
                }
                else
                {
#if UNITY_EDITOR
                    if (UMAABMSettings.GetEncryptionEnabled())
                    {
                        if (Debug.isDebugBuild)
                            Debug.LogWarning("You have AssetBunlde Encryption turned ON in UMAAssetBundleManager but have not enabled it in this scenes Dynamic AssetLoader. Please do this in the inspector for the DynamicAssetLoader component.");
                    }
#endif
                }
                InitializeSourceURL();//in the editor this might set AssetBundleManager.SimulateAssetBundleInEditor to be true aswell so check that
#if UNITY_EDITOR
                if (AssetBundleManager.SimulateAssetBundleInEditor)
                {
                    isInitialized = true;
                    isInitializing = false;
                    if (gameObjectsToActivateOnInit.Count > 0)
                    {
                        for (var i = 0; i < gameObjectsToActivateOnInit.Count; i++)
                        {
                            gameObjectsToActivateOnInit[i].SetActive(true);
                        }
                        gameObjectsToActivateOnInit.Clear();
                    }
                    yield break;
                }
                else
#endif
                    //DnamicAssetLoader should still say its initialized even no remoteServer URL was set (either manually or by the LocalWebServer)
                    //because we still want to run normally to load assets from Resources
                    if (remoteServerURL == "")
                {
                    isInitialized = true;
                    isInitializing = false;
                    if (gameObjectsToActivateOnInit.Count > 0)
                    {
                        for (var i = 0; i < gameObjectsToActivateOnInit.Count; i++)
                        {
                            gameObjectsToActivateOnInit[i].SetActive(true);
                        }
                        gameObjectsToActivateOnInit.Clear();
                    }
                    yield break;
                }
                var request = AssetBundleManager.Initialize(useJsonIndex, remoteServerIndexURL);
                if (request != null)
                {
                    while (AssetBundleManager.IsOperationInProgress(request))
                    {
                        yield return null;
                    }
                    isInitializing = false;
                    if (AssetBundleManager.AssetBundleIndexObject != null)
                    {
                        isInitialized = true;
                        if (gameObjectsToActivateOnInit.Count > 0)
                        {
                            for (var i = 0; i < gameObjectsToActivateOnInit.Count; i++)
                            {
                                gameObjectsToActivateOnInit[i].SetActive(true);
                            }
                            gameObjectsToActivateOnInit.Clear();
                        }
                    }
                    else
                    {
                        //if we are in the editor this can only have happenned because the asset bundles were not built and by this point
                        //an error will have already been shown about that and AssetBundleManager.SimulationOverride will be true so we can just continue.
#if UNITY_EDITOR
                        if (AssetBundleManager.AssetBundleIndexObject == null && AssetBundleManager.SimulateAssetBundleInEditor)
                        {
                            isInitialized = true;
                            yield break;
                        }
#endif
                    }
                }
                else
                {
                    if (Debug.isDebugBuild)
                        Debug.LogWarning("AssetBundleManager failed to initialize correctly");
					//set this false so ReInitializing can happen
					isInitializing = false;
                }
            }
        }

        /// <summary>
        /// Load a single assetbundle (and its dependencies) asynchroniously and sets the Loading Messages.
        /// </summary>
        /// <param name="assetBundleToLoad"></param>
        /// <param name="loadingMsg"></param>
        /// <param name="loadedMsg"></param>
        public void LoadAssetBundle(string assetBundleToLoad, string loadingMsg = "", string loadedMsg = "", Action loaded = null)
        {
            var assetBundlesToLoadList = new List<string>();
            assetBundlesToLoadList.Add(assetBundleToLoad);
            LoadAssetBundles(assetBundlesToLoadList, loadingMsg, loadedMsg, loaded);
        }
        /// <summary>
        /// Load multiple assetbundles (and their dependencies) asynchroniously and sets the Loading Messages.
        /// </summary>
        /// <param name="assetBundlesToLoad"></param>
        /// <param name="loadingMsg"></param>
        /// <param name="loadedMsg"></param>
        public void LoadAssetBundles(string[] assetBundlesToLoad, string loadingMsg = "", string loadedMsg = "", Action loaded = null)
        {
            var assetBundlesToLoadList = new List<string>(assetBundlesToLoad);
            LoadAssetBundles(assetBundlesToLoadList, loadingMsg, loadedMsg, loaded);
        }
        /// <summary>
        /// Load multiple assetbundles (and their dependencies) asynchroniously and sets the Loading Messages.
        /// </summary>
        /// <param name="assetBundlesToLoad"></param>
        /// <param name="loadingMsg"></param>
        /// <param name="loadedMsg"></param>
        public void LoadAssetBundles(List<string> assetBundlesToLoad, string loadingMsg = "", string loadedMsg = "", Action loaded = null)
        {
#if UNITY_EDITOR
            if (AssetBundleManager.SimulateAssetBundleInEditor)
            {
                foreach (var requiredBundle in assetBundlesToLoad)
                {
                    SimulateLoadAssetBundle(requiredBundle);
                }
                return;
            }
#endif
            var assetBundlesToReallyLoad = new List<string>();
            foreach (var requiredBundle in assetBundlesToLoad)
            {
                UnityEngine.Debug.Log("DWC LoadAssetBundles " + requiredBundle + "IsAssetBundleDownloaded="  + AssetBundleManager.IsAssetBundleDownloaded(requiredBundle));
                if (!AssetBundleManager.IsAssetBundleDownloaded(requiredBundle))
                {
                    assetBundlesToReallyLoad.Add(requiredBundle);
                }
            }
            if (assetBundlesToReallyLoad.Count > 0)
            {
                assetBundlesDownloading = true;
                canCheckDownloadingBundles = false;
                StartCoroutine(LoadAssetBundlesAsync(assetBundlesToReallyLoad));
            }
        }
        /// <summary>
        /// Loads a list of asset bundles and their dependencies asynchroniously
        /// </summary>
        /// <param name="assetBundlesToLoad">List of Asset Bundles to load.</param>
        /// <param name="loadingMsg"></param>
        /// <param name="loadedMsg"></param>
        /// <returns></returns>
        protected IEnumerator LoadAssetBundlesAsync(List<string> assetBundlesToLoad, string loadingMsg = "", string loadedMsg = "", Action loaded = null)
        {
            #if UNITY_EDITOR
            if (AssetBundleManager.SimulateAssetBundleInEditor) yield break;
            #endif

            if (!isInitialized)
            {
                if (!isInitializing)
                {
                    yield return StartCoroutine(Initialize());
                }
                else
                {
                    while (isInitialized == false)
                    {
                        yield return null;
                    }
                }
            }
            UnityEngine.Debug.Log("DWC LoadAssetBundlesAsync after Init ");
            var bundlesInManifest = AssetBundleManager.AssetBundleIndexObject.GetAllAssetBundles();
            foreach (var assetBundleName in assetBundlesToLoad)
            {
                var assetBundleNameLower = assetBundleName.ToLower();
                UnityEngine.Debug.Log("DWC assetBundleNameLower " + assetBundleNameLower);
                foreach (var bundle in bundlesInManifest)
                {
                    if ((bundle == assetBundleNameLower || bundle.IndexOf(assetBundleNameLower + "/") > -1))
                    {
                        if (Debug.isDebugBuild)
                            Debug.Log("Started loading of " + bundle);
                        if (AssetBundleLoadingIndicator.Instance)
                            AssetBundleLoadingIndicator.Instance.Show(bundle, loadingMsg, "", loadedMsg);
                        UnityEngine.Debug.Log("DWC StartCoroutine(LoadAssetBundleAsync(bundle)); " + bundle);
                        StartCoroutine(LoadAssetBundleAsync(bundle));
                    }
                }
            }
            canCheckDownloadingBundles = true;
            assetBundlesDownloading = true;
            //yield return null;
        }
        /// <summary>
        /// Loads an asset bundle and its dependencies asynchroniously
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        //DOS NOTES: if the local server is turned off after it was on when AssetBundleManager was initialized
        //(like could happen in the editor or if you run a build that uses the local server but you have not started Unity and turned local server on)
        //then this wrongly says that the bundle has downloaded
#pragma warning disable 0219 //remove the warning that we are not using loadedBundle- since we want the error
        protected IEnumerator LoadAssetBundleAsync(string bundle)
        {
            var startTime = Time.realtimeSinceStartup;
            UnityEngine.Debug.Log("DWC In LoadAssetBundleAsync " + bundle);

            AssetBundleManager.LoadAssetBundle(bundle, false);
            string error = null;
            UnityEngine.Debug.Log("DWC before while (AssetBundleManager.GetLoadedAssetBundle " + bundle);
            while (AssetBundleManager.GetLoadedAssetBundle(bundle, out error) == null) //DWC dies in here
            {
                yield return null;
            }
            
            // DWC NEver Gets HERE
            UnityEngine.Debug.Log("DWC after while (AssetBundleManager.GetLoadedAssetBundle " + bundle + " " + error);

            var loadedBundle = AssetBundleManager.GetLoadedAssetBundle(bundle, out error);
            UnityEngine.Debug.Log("DWC 2nd AssetBundleManager.GetLoadedAssetBundle" + bundle + " " + error);

            var elapsedTime = Time.realtimeSinceStartup - startTime;
            if (Debug.isDebugBuild)
                Debug.Log(bundle + (!String.IsNullOrEmpty(error) ? " was not" : " was") + " loaded successfully in " + elapsedTime + " seconds");
			if (!String.IsNullOrEmpty(error))
			{
                if (Debug.isDebugBuild)
                    Debug.LogError("[DynamicAssetLoader] Bundle Load Error: " + error);

                yield break;
            }
            
            UnityEngine.Debug.Log("DWC GetAllDependencies" + bundle);

            //If this assetBundle contains UMATextRecipes we may need to trigger some post processing...
            //it may have downloaded some dependent bundles too so these may need processing aswell
            var dependencies = AssetBundleManager.AssetBundleIndexObject.GetAllDependencies(bundle);
            //DOS 04112016 so maybe what we need to do here is check the dependencies are loaded too
            if (dependencies.Length > 0)
            {
                for (var i = 0; i < dependencies.Length; i++)
                {
                    while (AssetBundleManager.IsAssetBundleDownloaded(dependencies[i]) == false)
                    {
                        yield return null;
                    }
                }
            }
            UnityEngine.Debug.Log("DWC After  GetAllDependencies" + bundle);
            DynamicCharacterSystem thisDCS = null;
            if (UMAContext.Instance != null)
            {
                thisDCS = UMAContext.Instance.dynamicCharacterSystem as DynamicCharacterSystem;
            }
            
            UnityEngine.Debug.Log("DWC After  thisDCS" + bundle);
            
            if (thisDCS != null)
            {
                if (thisDCS.addAllRecipesFromDownloadedBundles)
                {
                    if (AssetBundleManager.AssetBundleIndexObject.GetAllAssetsOfTypeInBundle(bundle, "UMATextRecipe").Length > 0)
                    {
                        UnityEngine.Debug.Log("DWC if  GetAllAssetsOfTypeInBundle" + bundle);
                        //DCSRefresh only needs to be called if the downloaded asset bundle contained UMATextRecipes (or character recipes)
                        //Also it actually ONLY needs to search this bundle
                        thisDCS.Refresh(false, bundle);
                    }
                    UnityEngine.Debug.Log("DWC After  GetAllAssetsOfTypeInBundle" + bundle);
                    for (var i = 0; i < dependencies.Length; i++)
                    {
                        if (AssetBundleManager.AssetBundleIndexObject.GetAllAssetsOfTypeInBundle(dependencies[i], "UMATextRecipe").Length > 0)
                        {
                            UnityEngine.Debug.Log("DWC for thisDCS.Refresh" + bundle + "dependency" + dependencies[i]);
                            //DCSRefresh only needs to be called if the downloaded asset bundle contained UMATextRecipes (or character recipes)
                            //Also it actually ONLY needs to search this bundle
                            thisDCS.Refresh(false, dependencies[i]);
                        }
                    }
                }
            }
            UnityEngine.Debug.Log("BundleLoaded?.Invoke(bundle);" + bundle);
            BundleLoaded?.Invoke(bundle);
        }
#pragma warning restore 0219

        #endregion

        #region LOAD ASSETS METHODS
        [HideInInspector]
        public bool debugOnFail = true;
        public bool AddAssets<T>(ref Dictionary<string, List<string>> assetBundlesUsedDict, bool searchResources, bool searchBundles, bool downloadAssetsEnabled, string bundlesToSearch = "", string resourcesFolderPath = "", int? assetNameHash = null, string assetName = "", Action<T[]> callback = null, bool forceDownloadAll = false) where T : UnityEngine.Object
        {
            if (isInitialized == false && Application.isPlaying && (searchBundles && downloadAssetsEnabled))
            {
                if (Debug.isDebugBuild)
                    Debug.LogWarning("[DynamicAssetLoader] had not finished initializing when " + typeof(T).ToString() + " assets were requested. Please be sure wait for 'DynamicAssetLoader.Instance.isInitialized' to be true before requesting dynamically addedd assets from the libraries.");
            }
            var found = false;
            var assetsToReturn = new List<T>();
            var resourcesFolderPathArray = SearchStringToArray(resourcesFolderPath);
            var bundlesToSearchArray = SearchStringToArray(bundlesToSearch);
            //search UMA AssetIndex
            if (searchResources)
            {
                //using UMAAssetIndexer!!
                if (UMAAssetIndexer.Instance != null)
                {
                    found = AddAssetsFromResourcesIndex<T>(ref assetsToReturn, resourcesFolderPathArray, assetNameHash, assetName);
                }
                else
                {
                    if (Debug.isDebugBuild)
                        Debug.LogWarning("[DynamicAssetLoader] UMAResourcesIndex.Instance WAS NULL");
                }
            }
            //if we can and want to search asset bundles
            if ((AssetBundleManager.AssetBundleIndexObject != null || AssetBundleManager.SimulateAssetBundleInEditor == true) || Application.isPlaying == false)
                if (searchBundles && (found == false || (assetName == "" && assetNameHash == null)))
                {
                    var foundHere = AddAssetsFromAssetBundles<T>(ref assetBundlesUsedDict, ref assetsToReturn, downloadAssetsEnabled, bundlesToSearchArray, assetNameHash, assetName, callback, forceDownloadAll);
                    found = foundHere == true ? true : found;
                }
            if (callback != null && assetsToReturn.Count > 0)
            {
                callback(assetsToReturn.ToArray());
            }
            return found;
        }

        public bool AddAssets<T>(bool searchResources, bool searchBundles, bool downloadAssetsEnabled, string bundlesToSearch = "", string resourcesFolderPath = "", int? assetNameHash = null, string assetName = "", Action<T[]> callback = null, bool forceDownloadAll = false) where T : UnityEngine.Object
        {
            var dummyDict = new Dictionary<string, List<string>>();
            return AddAssets<T>(ref dummyDict, searchResources, searchBundles, downloadAssetsEnabled, bundlesToSearch, resourcesFolderPath, assetNameHash, assetName, callback, forceDownloadAll);
        }

        public bool AddAssetsFromResourcesIndex<T>(ref List<T> assetsToReturn, string[] resourcesFolderPathArray, int? assetNameHash = null, string assetName = "") where T : UnityEngine.Object
        {
            var found = false;
            //Use new UMAAssetIndexer!!
            if (UMAAssetIndexer.Instance == null)
                return found;
            if (assetNameHash != null || assetName != "")
            {
                T foundAsset = null;
                if (assetNameHash != null)
                {
                    //using UMAAssetIndexer
                    foundAsset = (UMAAssetIndexer.Instance.GetAsset<T>((int)assetNameHash, resourcesFolderPathArray) as T);
                }
                else if (assetName != "")
                {
					//check if its a Placeholder asset that has been requested directly- this happens when the UMATextRecipePlaceholder tries to load
					var typePlaceholderName = typeof(T).ToString().Replace(typeof(T).Namespace + ".", "") + "Placeholder";
					if (typePlaceholderName == assetName || typePlaceholderName + "_Slot" == assetName)
					{
						foundAsset = GetPlaceholderAsset<T>(assetName);
					}
					//using UMAAssetIndexer
					if (foundAsset == null)
						foundAsset = (UMAAssetIndexer.Instance.GetAsset<T>(assetName, resourcesFolderPathArray) as T);
				}
				if (foundAsset != null)
                {
                    assetsToReturn.Add(foundAsset);
                    found = true;
                }
            }
            else if (assetNameHash == null && assetName == "")
            {
				//Using UMAAssetIndexer
				var assetIndexerAssets = UMAAssetIndexer.Instance.GetAllAssets<T>(resourcesFolderPathArray) as List<T>;
				var assetIndexerAssetsToAdd = new List<T>();
				//UMAAssetIndexer returns null assets so check for that
				if (assetIndexerAssets.Count > 0)
				{
					for (var i = 0; i < assetIndexerAssets.Count; i++)
					{
						if (assetIndexerAssets[i] != null)
						{
							assetIndexerAssetsToAdd.Add(assetIndexerAssets[i]);
						}
					}
				}
				assetsToReturn.AddRange(assetIndexerAssetsToAdd);
				found = assetsToReturn.Count > 0;
            }
            return found;
        }

		public T GetPlaceholderAsset<T>(string placeholderName) where T : UnityEngine.Object
		{
			if (placeholderName.IndexOf("Placeholder") == -1)
				return null;
			return (T)Resources.Load<T>("PlaceholderAssets/" + placeholderName) as T;
		}

		/// <summary>
		/// Generic Library function to search AssetBundles for a type of asset, optionally filtered by bundle name, and asset assetNameHash or assetName.
		/// Optionally sends the found assets to the supplied callback for processing.
		/// Automatically performs the operation in SimulationMode if AssetBundleManager.SimulationMode is enabled or if the Application is not playing.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assetBundlesUsedDict"></param>
		/// <param name="assetsToReturn"></param>
		/// <param name="downloadAssetsEnabled"></param>
		/// <param name="bundlesToSearchArray"></param>
		/// <param name="assetNameHash"></param>
		/// <param name="assetName"></param>
		/// <param name="callback"></param>
		/// <param name="forceDownloadAll"></param>
		public bool AddAssetsFromAssetBundles<T>(ref Dictionary<string, List<string>> assetBundlesUsedDict, ref List<T> assetsToReturn, bool downloadAssetsEnabled, string[] bundlesToSearchArray, int? assetNameHash = null, string assetName = "", Action<T[]> callback = null, bool forceDownloadAll = false) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (AssetBundleManager.SimulateAssetBundleInEditor)
            {
                return SimulateAddAssetsFromAssetBundlesNew<T>(ref assetBundlesUsedDict, ref assetsToReturn, bundlesToSearchArray, assetNameHash, assetName, callback, forceDownloadAll);
            }
            else
            {
#endif
                if (AssetBundleManager.AssetBundleIndexObject == null)
                {
#if UNITY_EDITOR
                    if (Debug.isDebugBuild)
                        Debug.LogWarning("[DynamicAssetLoader] No AssetBundleManager.AssetBundleIndexObject found. Do you need to rebuild your AssetBundles and/or upload the platform index bundle?");
                    AssetBundleManager.SimulateOverride = true;
                    return SimulateAddAssetsFromAssetBundlesNew<T>(ref assetBundlesUsedDict, ref assetsToReturn, bundlesToSearchArray, assetNameHash, assetName, callback);
#else
                    if(Debug.isDebugBuild)
					    Debug.LogError("[DynamicAssetLoader] No AssetBundleManager.AssetBundleIndexObject found. Do you need to rebuild your AssetBundles and/or upload the platform index bundle?");
                    return false;
#endif
                }
                var allAssetBundleNames = AssetBundleManager.AssetBundleIndexObject.GetAllAssetBundleNames();
                var assetBundleNamesArray = allAssetBundleNames;
                var typeParameterType = typeof(T);
                var typeString = typeParameterType.FullName;
                if (bundlesToSearchArray.Length > 0 && bundlesToSearchArray[0] != "")
                {
                    var processedBundleNamesArray = new List<string>();
                    for (var i = 0; i < bundlesToSearchArray.Length; i++)
                    {
                        for (var ii = 0; ii < allAssetBundleNames.Length; ii++)
                        {
                            if (allAssetBundleNames[ii].IndexOf(bundlesToSearchArray[i]) > -1 && !processedBundleNamesArray.Contains(allAssetBundleNames[ii]))
                            {
                                processedBundleNamesArray.Add(allAssetBundleNames[ii]);
                            }
                        }
                    }
                    assetBundleNamesArray = processedBundleNamesArray.ToArray();
                }
                var assetFound = false;
                for (var i = 0; i < assetBundleNamesArray.Length; i++)
                {
                    var error = "";
                    if (assetNameHash != null && assetName == "")
                    {
                        assetName = AssetBundleManager.AssetBundleIndexObject.GetAssetNameFromHash(assetBundleNamesArray[i], assetNameHash, typeString);
                    }
                    if (assetName != "" || assetNameHash != null)
                    {
                        if (assetName == "" && assetNameHash != null)
                        {
                            continue;
                        }
                        var assetBundleContains = AssetBundleManager.AssetBundleIndexObject.AssetBundleContains(assetBundleNamesArray[i], assetName, typeString);
                        if (assetBundleContains)
                        {
                            if (AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error) != null)
                            {
                                var assetFilename = AssetBundleManager.AssetBundleIndexObject.GetFilenameFromAssetName(assetBundleNamesArray[i], assetName, typeString).ToLower();
                                var loadedBundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error);
                                var unityBundle = loadedBundle.m_AssetBundle;
                                var asset = unityBundle.LoadAsset(assetFilename);
                                var target = asset as T;
                                if (target != null)
                                {
                                    assetFound = true;
                                    if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
                                    {
                                        assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
                                    }
                                    if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(assetName))
                                    {
                                        assetBundlesUsedDict[assetBundleNamesArray[i]].Add(assetName);
                                    }
                                    assetsToReturn.Add(target);
                                    if (assetName != "")
                                        break;
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(error))
                                    {
                                        if (Debug.isDebugBuild)
                                            Debug.LogWarning(error);
                                    }
                                }
                            }
                            else if (downloadAssetsEnabled)
                            {
                                //Here we return a temp asset and wait for the bundle to download
                                //We dont want to create multiple downloads of the same bundle so check its not already downloading
                                if (AssetBundleManager.AreBundlesDownloading(assetBundleNamesArray[i]) == false)
                                {
                                    LoadAssetBundle(assetBundleNamesArray[i]);
                                }
                                else
                                {
                                    //do nothing its already downloading
                                }
                                if (assetNameHash == null)
                                {
                                    assetNameHash = AssetBundleManager.AssetBundleIndexObject.GetAssetHashFromName(assetBundleNamesArray[i], assetName, typeString);
                                }
                                var target = downloadingAssets.AddDownloadItem<T>(assetName, assetNameHash, assetBundleNamesArray[i], callback);
                                if (target != null)
                                {
                                    assetFound = true;
                                    if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
                                    {
                                        assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
                                    }
                                    if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(assetName))
                                    {
                                        assetBundlesUsedDict[assetBundleNamesArray[i]].Add(assetName);
                                    }
                                    assetsToReturn.Add(target);
                                    if (assetName != "")
                                        break;
                                }
                            }
                        }
                    }
                    else //we are just loading in all assets of type from the downloaded bundles- if you are happy to trigger the download of all possible assetbundles that contain anything of type T set forceDownloadAll to be true
                    {
                        if (AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error) != null)
                        {
                            var assetsInBundle = AssetBundleManager.AssetBundleIndexObject.GetAllAssetsOfTypeInBundle(assetBundleNamesArray[i], typeString);
                            if (assetsInBundle.Length > 0)
                            {
                                foreach (var asset in assetsInBundle)
                                {
                                    //sometimes this errors out if the bundle is downloaded but not LOADED
                                    var assetFilename = AssetBundleManager.AssetBundleIndexObject.GetFilenameFromAssetName(assetBundleNamesArray[i], asset, typeString).ToLower();
                                    T target = null;
                                    try
                                    {
                                        var loadedBundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error);
                                        var unityBundle = loadedBundle.m_AssetBundle;
                                        var assetBundleName = assetBundleNamesArray[i];
                                        string fullName = assetFilename;
                                        foreach (var path in unityBundle.GetAllAssetNames())
                                        {
                                            if (path.Contains(assetFilename))
                                            {
                                                fullName = path;
                                            }
                                        }
                                        var bAsset = unityBundle.LoadAsset(assetFilename);
                                        target = bAsset as T;
                                    }
                                    catch
                                    {
                                        if (Debug.isDebugBuild)
                                            Debug.LogWarning("[DynamicAssetLoader]AddAssetsFromAssetBundles " + assetBundleNamesArray[i] + " had not loaded at the time of the request");
                                        var thiserror = "";
                                        AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out thiserror);
                                        if (thiserror != "" && thiserror != null)
                                        {
                                            if (Debug.isDebugBuild)
                                                Debug.LogWarning("GetLoadedAssetBundle error was " + thiserror);
                                        }
                                        else if (AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out thiserror).m_AssetBundle == null)
                                        {
                                            //The problem is here the bundle is downloaded but not LOADED
                                            if (Debug.isDebugBuild)
                                                Debug.LogWarning("Bundle was ok but m_AssetBundle was null");
                                        }
                                        else if (AssetBundleManager.GetLoadedAssetBundle(assetBundleNamesArray[i], out error).m_AssetBundle.LoadAsset<T>(asset) == null)
                                        {
                                            if (Debug.isDebugBuild)
                                                Debug.LogWarning("Load Asset could not get a " + typeof(T).ToString() + " asset called " + asset + " from " + assetBundleNamesArray[i]);
                                        }
                                    }
                                    if (target != null)
                                    {
                                        assetFound = true;
                                        if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
                                        {
                                            assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
                                        }
                                        if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(asset))
                                        {
                                            assetBundlesUsedDict[assetBundleNamesArray[i]].Add(asset);
                                        }
                                        assetsToReturn.Add(target);
                                    }
                                    else
                                    {
                                        if (!String.IsNullOrEmpty(error))
                                        {
                                            if (Debug.isDebugBuild)
                                                Debug.LogWarning(error);
                                        }
                                    }
                                }
                            }
                        }
                        else if (forceDownloadAll && downloadAssetsEnabled)//if its not downloaded but we are forcefully downloading any bundles that contain a type of asset make it download and add the temp asset to the downloading assets list
                        {
                            var assetsInBundle = AssetBundleManager.AssetBundleIndexObject.GetAllAssetsOfTypeInBundle(assetBundleNamesArray[i], typeString);
                            if (assetsInBundle.Length > 0)
                            {
                                //Debug.Log("[DynamicAssetLoader] forceDownloadAll was true for " + typeString + " and found in " + assetBundleNamesArray[i]);
                                for (var aib = 0; aib < assetsInBundle.Length; aib++)
                                {
                                    //Here we return a temp asset and wait for the bundle to download
                                    //We dont want to create multiple downloads of the same bundle so check its not already downloading
                                    if (AssetBundleManager.AreBundlesDownloading(assetBundleNamesArray[i]) == false)
                                    {
                                        LoadAssetBundle(assetBundleNamesArray[i]);
                                    }
                                    else
                                    {
                                        //do nothing its already downloading
                                    }
                                    var thisAssetName = assetsInBundle[aib];
                                    var thisAssetNameHash = AssetBundleManager.AssetBundleIndexObject.GetAssetHashFromName(assetBundleNamesArray[i], thisAssetName, typeString);
                                    var target = downloadingAssets.AddDownloadItem<T>(/*CurrentBatchID,*/ thisAssetName, thisAssetNameHash, assetBundleNamesArray[i], callback/*, requestingUMA*/);
                                    if (target != null)
                                    {
                                        if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
                                        {
                                            assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
                                        }
                                        if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(thisAssetName))
                                        {
                                            assetBundlesUsedDict[assetBundleNamesArray[i]].Add(thisAssetName);
                                        }
                                        assetsToReturn.Add(target);
                                    }
                                }
                            }
                        }
                    }
                }
                if (!assetFound && assetName != "" && debugOnFail)
                {
                    var assetIsInArray = AssetBundleManager.AssetBundleIndexObject.FindContainingAssetBundle(assetName, typeString);
                    var assetIsIn = assetIsInArray.Length > 0 ? " but it was in " + assetIsInArray[0] : ". Do you need to reupload you platform manifest and index?";
                    if (Debug.isDebugBuild)
                        Debug.LogWarning("Dynamic" + typeof(T).Name + "Library (" + typeString + ") could not load " + assetName + " from any of the AssetBundles searched" + assetIsIn);
                }

                return assetFound;
#if UNITY_EDITOR
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Simulates the loading of assets when AssetBundleManager is set to 'SimulationMode'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundlesUsedDict"></param>
        /// <param name="assetsToReturn"></param>
        /// <param name="bundlesToSearchArray"></param>
        /// <param name="assetNameHash"></param>
        /// <param name="assetName"></param>
        /// <param name="callback"></param>
        /// <param name="forceDownloadAll"></param>
        bool SimulateAddAssetsFromAssetBundlesNew<T>(ref Dictionary<string, List<string>> assetBundlesUsedDict, ref List<T> assetsToReturn, string[] bundlesToSearchArray, int? assetNameHash = null, string assetName = "", Action<T[]> callback = null, bool forceDownloadAll = false) where T : UnityEngine.Object
        {
            var st = UMAAssetIndexer.StartTimer();
			var assetNameToSearch = assetName;
            var typeParameterType = typeof(T);
            var typeString = typeParameterType.FullName;
            var currentSimulatedDownloadedBundlesCount = simulatedDownloadedBundles.Count;
            if (assetNameHash != null)
            {
                // We could load all assets of type, iterate over them and get the hash and see if it matches...But then that would be as slow as loading from resources was
                if (Debug.isDebugBuild)
                    Debug.Log("It is not currently possible to search for assetBundle assets in SimulationMode using the assetNameHash. " + typeString + " is trying to do this with assetNameHash " + assetNameHash);
            }
            var allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            string[] assetBundleNamesArray;
            if (bundlesToSearchArray.Length > 0 && bundlesToSearchArray[0] != "")
            {
                var processedBundleNamesArray = new List<string>();
                for (var i = 0; i < bundlesToSearchArray.Length; i++)
                {
                    for (var ii = 0; ii < allAssetBundleNames.Length; ii++)
                    {
                        if (allAssetBundleNames[ii].IndexOf(bundlesToSearchArray[i]) > -1 && !processedBundleNamesArray.Contains(allAssetBundleNames[ii]))
                        {
                            processedBundleNamesArray.Add(allAssetBundleNames[ii]);
                        }
                    }
                }
                assetBundleNamesArray = processedBundleNamesArray.ToArray();
            }
            else
            {
                assetBundleNamesArray = allAssetBundleNames;
            }
            var assetFound = false;
            //a list of all the assets any assets we load depend on
            var dependencies = new List<string>();
            for (var i = 0; i < assetBundleNamesArray.Length; i++)
            {
                if (assetFound && assetName != "")//Do we want to break actually? What if the user has named two overlays the same? Or would this not work anyway?
                    break;
                var possiblePaths = new string[0];
                if (assetName != "")
                {
                    //This is a compromise for the sake of speed that assumes slot/overlay/race assets have the same slotname/overlayname/racename as their actual asset
                    //if we dont do this we have to load all the assets of that type and check their name which is really slow
                    //I think its worth having this compromise because this does not happen when the local server is on or the assets are *actually* downloaded from an external source because the AssetBundleIndex is used then
                    //if this is looking for SlotsDataAssets then the asset name has _Slot after it usually even if the slot name doesn't have that-but the user might have renamed it so cover both cases
					//THIS COMPROMISE DOES NOT WORK WITH UMA CORE CONTENT the asset for 'MaleEyes' for example is 'UMA_Human_Male_Eyes_Slot'
					//I think the only way to make this quick is for the GlobalLibrary to maintain a list of UMA assets that are in AssetBundles for use in the editor when we are in 'SimulationMode'
					/*if (typeof(T) == typeof(SlotDataAsset))
                    {
                        string[] possiblePathsTemp = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleNamesArray[i], assetName);
                        string[] possiblePaths_SlotTemp = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleNamesArray[i], assetName + "_Slot");
                        List<string> possiblePathsList = new List<string>(possiblePathsTemp);
                        foreach (string path in possiblePaths_SlotTemp)
                        {
                            if (!possiblePathsList.Contains(path))
                            {
                                possiblePathsList.Add(path);
                            }
                        }
                        possiblePaths = possiblePathsList.ToArray();
                    }
                    else
                    {
                        possiblePaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleNamesArray[i], assetName);
                    }*/
					//not sure how we can do anything here other than load _everything_ and see if its what we want- blasted filters just dont seem to work
					if (typeof(T) == typeof(SlotDataAsset) || typeof(T) == typeof(OverlayDataAsset) || typeof(T) == typeof(RaceData))
					{
						//This works (slowly) but also causes ALL the assets from the assetBundle to be loaded
						//which is not ideal but then this is editor only so maybe not an issue?
						var possiblePathsTempTemp = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNamesArray[i]);
						var possiblePathsTempList = new List<string>();
						foreach (var path in possiblePathsTempTemp)
                    {
							var containingPath = System.IO.Path.GetDirectoryName(path);
							var typeGUIDs = AssetDatabase.FindAssets("t:" + typeof(T).ToString().Replace(typeof(T).Namespace + ".", ""), new string[1] { containingPath });
							for (var ti = 0; ti < typeGUIDs.Length; ti++)
								possiblePathsTempList.Add(AssetDatabase.GUIDToAssetPath(typeGUIDs[ti]));
						}
						var possiblePathsTemp = possiblePathsTempList.ToArray();
						var possiblePathsList = new List<string>();
						T tempTarget = null;
                        for (var pti = 0; pti < possiblePathsTemp.Length; pti++)
                        {
							tempTarget = (T)AssetDatabase.LoadAssetAtPath(possiblePathsTemp[pti], typeof(T));
							if(tempTarget)
							if(
								(typeof(T) == typeof(SlotDataAsset) && (tempTarget as SlotDataAsset).slotName == assetName)||
								(typeof(T) == typeof(OverlayDataAsset) && (tempTarget as OverlayDataAsset).overlayName == assetName)||
								(typeof(T) == typeof(RaceData) && (tempTarget as RaceData).raceName == assetName)
								)
                            {
								possiblePathsList.Add(possiblePathsTemp[pti]);
                            }
                        }
                        possiblePaths = possiblePathsList.ToArray();
						assetNameToSearch = "";//we cant use the sent name as a filter because its a slot/overlay/racename
                    }
                    else
                    {
                        possiblePaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleNamesArray[i], assetName);
                    }
                }
                else
                {
                    //if the application is not playing we want to load ALL the assets from the bundle this asset will be in
                    if (!Application.isPlaying)
                    {
                        possiblePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNamesArray[i]);
                    }
                    else if (simulatedDownloadedBundles.Contains(assetBundleNamesArray[i]) || forceDownloadAll)
                    {
                        //DCS.Refresh calls for assets without sending a name and in reality this just checks bundles that are already downloaded
                        //this mimics that behaviour
                        possiblePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNamesArray[i]);
                    }
                }
                foreach (var path in possiblePaths)
                {
					// the line T target = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T)); below appears to load the asset *and then* check if its a type of T
					// this leads to a big slowdown the first time this happens. Its much quicker (although messier) to do the following as getting the paths
					// does not load the actual asset. This is also slightly quicker than getting all paths of type T outside this loop
					// the 't:' filter needs the type to not have a namespace
					var typeForSearch = typeof(T).ToString().Replace(typeof(T).Namespace + ".", "");
					var searchString = assetNameToSearch == "" ? "t:" + typeForSearch : "t:" + typeForSearch + " " + assetName;
					var containingPath = System.IO.Path.GetDirectoryName(path);
					var typeGUIDs = AssetDatabase.FindAssets(searchString, new string[1] { containingPath });
					var typePaths = new List<string>(typeGUIDs.Length);
					for (var ti = 0; ti < typeGUIDs.Length; ti++)
						typePaths.Add(AssetDatabase.GUIDToAssetPath(typeGUIDs[ti]));

					if (!typePaths.Contains(path))
						continue;

					var target = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
                    if (target != null)
                    {
                        assetFound = true;
                        if (!assetBundlesUsedDict.ContainsKey(assetBundleNamesArray[i]))
                        {
                            assetBundlesUsedDict[assetBundleNamesArray[i]] = new List<string>();
                        }
                        if (!assetBundlesUsedDict[assetBundleNamesArray[i]].Contains(assetName))
                        {
                            assetBundlesUsedDict[assetBundleNamesArray[i]].Add(assetName);
                        }
                        assetsToReturn.Add(target);
                        //Add the bundle this asset was in to the simulatedDownloadedBundles list if its not already there
                        if (!simulatedDownloadedBundles.Contains(assetBundleNamesArray[i]))
                            simulatedDownloadedBundles.Add(assetBundleNamesArray[i]);
                        //Find the dependencies for all the assets in this bundle because AssetBundleManager would automatically download those bundles too
                        //Dont bother finding dependencies when something is just trying to load all asset of type
                        if (Application.isPlaying && assetName != "" && forceDownloadAll == false)
                        {
                            var thisDependencies = AssetDatabase.GetDependencies(path, false);
                            for (var depi = 0; depi < thisDependencies.Length; depi++)
                            {
                                if (!dependencies.Contains(thisDependencies[depi]))
                                {
                                    dependencies.Add(thisDependencies[depi]);
                                }
                            }
                        }
                        if (assetName != "")
                            break;
                    }
                }
            }
            if (dependencies.Count > 0 && assetName != "" && forceDownloadAll == false)
            {
                //we need to load ALL the assets from every Assetbundle that has a dependency in it.
                var AssetBundlesToFullyLoad = new List<string>();
                for (var i = 0; i < assetBundleNamesArray.Length; i++)
                {
                    var allAssetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleNamesArray[i]);
                    var processed = false;
                    for (var ii = 0; ii < allAssetBundlePaths.Length; ii++)
                    {
                        for (var di = 0; di < dependencies.Count; di++)
                        {
                            if (allAssetBundlePaths[ii] == dependencies[di])
                            {
                                if (!AssetBundlesToFullyLoad.Contains(assetBundleNamesArray[i]))
                                    AssetBundlesToFullyLoad.Add(assetBundleNamesArray[i]);
                                //Add this bundle to the simulatedDownloadedBundles list if its not already there because that would have been downloaded too
                                if (!simulatedDownloadedBundles.Contains(assetBundleNamesArray[i]))
                                    simulatedDownloadedBundles.Add(assetBundleNamesArray[i]);
                                processed = true;
                                break;
                            }
                        }
                        if (processed) break;
                    }
                }
            }
            if (!assetFound && assetName != "" && debugOnFail)
            {
                if (Debug.isDebugBuild)
                    Debug.LogWarning("Dynamic" + typeString + "Library could not simulate the loading of " + assetName + " from any AssetBundles");

                return assetFound;
            }
            if (assetsToReturn.Count > 0 && callback != null)
            {
                callback(assetsToReturn.ToArray());
            }
            //Racedata will trigger an update of DCS itself if it added a race DCS needs to know about
            //Other assets may have caused psuedo downloads of bundles DCS should check for UMATextRecipes in
            //Effectively this mimics DynamicAssetLoader loadAssetBundleAsyncs call of DCS.Refresh
            //- but without loading all the assets to check if any of them are UMATextRecipes because that is too slow
            //10012017 Only do this if thisDCS.addAllRecipesFromDownloadedBundles is true
            if (currentSimulatedDownloadedBundlesCount != simulatedDownloadedBundles.Count /*&& typeof(T) != typeof(RaceData)*/ && assetName != "")
            {
                var thisDCS = UMAContext.Instance.dynamicCharacterSystem as DynamicCharacterSystem;
                if (thisDCS != null)
                {
                    if (thisDCS.addAllRecipesFromDownloadedBundles)
                    {
                        //but it only needs to add stuff from the bundles that were added
                        for (var i = currentSimulatedDownloadedBundlesCount; i < simulatedDownloadedBundles.Count; i++)
                        {
                            thisDCS.Refresh(false, simulatedDownloadedBundles[i]);
                        }
                    }
                }
            }
            UMAAssetIndexer.StopTimer(st, "SimulateAddAssetsFromAssetBundlesNew Type=" + typeof(T).Name);
            return assetFound;
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Mimics the check dynamicAssetLoader does when an actual LoadBundleAsync happens
        /// where it checks if the asset has any UMATextRecipes in it and if it does makes DCS.Refresh to get them
        /// </summary>
        /// <param name="assetBundleToLoad"></param>
        //10012017 Only do this if thisDCS.addAllRecipesFromDownloadedBundles is true
        public void SimulateLoadAssetBundle(string assetBundleToLoad)
        {
            var bundleAlreadySimulated = true;
            if (!simulatedDownloadedBundles.Contains(assetBundleToLoad))
            {
                simulatedDownloadedBundles.Add(assetBundleToLoad);
                bundleAlreadySimulated = false;
            }
            var allAssetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleToLoad);
            //We need to add the recipes from the bundle to DCS, other assets add them selves as they are requested by the recipes
            var thisDCS = UMAContext.Instance.dynamicCharacterSystem as DynamicCharacterSystem;
            var dcsNeedsRefresh = false;
            if (thisDCS)
            {
                if (thisDCS.addAllRecipesFromDownloadedBundles)
                {
                    for (var i = 0; i < allAssetBundlePaths.Length; i++)
                    {
                        var obj = AssetDatabase.LoadMainAssetAtPath(allAssetBundlePaths[i]);
                        if (obj.GetType() == typeof(UMATextRecipe))
                        {
                            if (bundleAlreadySimulated == false)
                                dcsNeedsRefresh = true;
                            break;
                        }
                    }
                    if (dcsNeedsRefresh)
                    {
                        thisDCS.Refresh(false, assetBundleToLoad);
                    }
                }
            }
        }
#endif
        /// <summary>
        /// Splits the 'ResourcesFolderPath(s)' and 'AssetBundleNamesToSearch' fields up by comma if the field is using that functionality...
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        string[] SearchStringToArray(string searchString = "")
        {
            string[] searchArray;
            if (String.IsNullOrEmpty(searchString))
            {
                searchArray = new string[] { "" };
            }
            else
            {
                searchString.Replace(" ,", ",").Replace(", ", ",");
                if (searchString.IndexOf(",") == -1)
                {
                    searchArray = new string[1] { searchString };
                }
                else
                {
                    searchArray = searchString.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            return searchArray;
        }

        #endregion

        #region SPECIAL TYPES
        //DownloadingAssetsList and DownloadingAssetItem moved into their own scripts to make this one a bit more manageable!        
        #endregion
    }
}
