using System.Collections;
using Common;
using UnityEngine;
using UnityEngine.XR.Management;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class XRInitializer: MonoBehaviour
    {
        #if !UNITY_EDITOR
        private void Start()
        {
            CoroutineSource.Instance.StartCoroutine(StartXRCoroutine());
        }
        
        private IEnumerator StartXRCoroutine()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogError("### Initializing XR Failed. Check Editor or Player log for details.");
            }
            else
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            }
        }
        //todo: would be good to control manually on enter/exit LE
        // void OnDisable()
        // {
        //     XRGeneralSettings.Instance.Manager.StopSubsystems();
        //     XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        //     Debug.Log("XR stopped completely.");
        // }
        #endif
    }
}