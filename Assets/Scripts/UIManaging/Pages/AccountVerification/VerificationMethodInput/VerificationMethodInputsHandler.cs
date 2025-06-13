using System;
using System.Collections;
using Bridge.AccountVerification.Models;
using Common.Abstract;
using Common.Collections;
using Extensions;
using Modules.AccountVerification;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.AccountVerification.VerificationMethodInput
{
    internal sealed class VerificationMethodInputsHandler: BaseContextPanel<IVerificationMethod>
    {
        [SerializeField] private VerificationMethodInputsDictionary _verificationMethodInputsMap;
        [SerializeField] private bool _showHeaders;
        [Header("Runtime size adjustment")]
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private Vector2 _minHeightRange  = new (150f, 225f);

        protected override bool IsReinitializable => true;
        
        private VerificationMethodInputPanel CurrentInputMethod { get; set; }
        
        public event Action<bool> InputValidated;

        public void ChangeVerificationMethod(IVerificationMethod method)
        {
            _verificationMethodInputsMap.Values.ForEach(panel => panel.SetActive(false));

            if (!_verificationMethodInputsMap.TryGetValue(method.Type, out var inputMethod))
            {
                Debug.LogError($"[{GetType().Name}] Verification input for {ContextData.Type} is not found");
                return;
            }

            if (CurrentInputMethod != null && CurrentInputMethod.IsInitialized)
            {
                ReleaseCurrentMethod();
            }
            
            CurrentInputMethod = inputMethod;
            CurrentInputMethod.Initialize(new VerificationMethodInputViewModel(method, _showHeaders));
            
            CurrentInputMethod.InputValidated += OnInputValidated;
            
            CurrentInputMethod.SetActive(true);
        }

        public void ShowValidationError(string error)
        {
            _layoutElement.minHeight = _minHeightRange.y;
            
            CurrentInputMethod.ShowValidationError(error);
        }

        public void Select()
        {
            StartCoroutine(DelayedSelectionRoutine());
        }

        protected override void OnInitialized() => ChangeVerificationMethod(ContextData);

        protected override void BeforeCleanUp() => ReleaseCurrentMethod();

        private void ReleaseCurrentMethod()
        {
            CurrentInputMethod.InputValidated -= OnInputValidated;
            
            CurrentInputMethod.CleanUp();
            CurrentInputMethod = null;
        }
        
        private void OnInputValidated(bool isValid)
        {
            _layoutElement.minHeight = _minHeightRange.x;
            
            InputValidated?.Invoke(isValid);
        }

        private IEnumerator DelayedSelectionRoutine()
        {
            while (!CurrentInputMethod.gameObject.activeSelf)
            {
                yield return null;
            }
            
            CurrentInputMethod.Select();
        }
    }

    [Serializable]
    internal sealed class VerificationMethodInputsDictionary : SerializedDictionary<CredentialType, VerificationMethodInputPanel>
    {
    }
}