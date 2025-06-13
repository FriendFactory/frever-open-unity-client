using System;
using System.Collections;
using Extensions;
using Navigation.Core;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.RegistrationInputFields;
using UIManaging.Pages.OnBoardingPage.UI.Args;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.VerificationCodePage
{
    internal sealed class VerificationCodePage : GenericPage<VerificationCodeArgs>
    {
        private const int WAIT_TIME_TO_RESEND = 60;
        private const int VERIFICATION_CODE_LENGTH = 6;

        [SerializeField] private Button _backButton;
        [SerializeField] private Button _continueButton;
        
        [Space]
        [SerializeField] private TMP_Text _description;
        [SerializeField] private SpecializedInputFieldBase _inputField;
        
        [Space]
        [SerializeField] private Button _sendVerificationCodeButton;
        [SerializeField] private TextMeshProUGUI _sendVerificationCodeText;
        [SerializeField] private GameObject _loadingIndicator;

        [SerializeField] private TMP_Text _validationText;

        [Inject] private OnBoardingLocalization _onBoardingLocalization;
        
        private Coroutine _resendCodeCoroutine;
        private int _timeToResendPossible;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
         
        public override PageId Id => PageId.VerificationPage;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void Awake()
        {
            _continueButton.onClick.AddListener(OnContinueButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override void OnDisplayStart(VerificationCodeArgs args)
        {
            _description.text = args.Description;
            _validationText.SetActive(false);
            
            var inputFieldModel = new SpecializedInputFieldModel
            {
                InitialText = string.Empty,
                PlaceHolderText = string.Empty,
                InputType = TMP_InputField.InputType.Standard,
                ContentType = TMP_InputField.ContentType.IntegerNumber,
                CharacterLimit = VERIFICATION_CODE_LENGTH,
                OnValueChanged = OnValueChanged,
                OnKeyboardSubmit = OnContinueButtonClicked
            };
            _inputField.Initialize(inputFieldModel);
            _inputField.Display();
            
            base.OnDisplayStart(args);
            _sendVerificationCodeButton.onClick.AddListener(ResendVerificationCodeButtonClick);
            _sendVerificationCodeButton.SetActive(true);
            _continueButton.SetActive(false);
            _timeToResendPossible = WAIT_TIME_TO_RESEND;
            _sendVerificationCodeText.text = string.Format(_onBoardingLocalization.ResendInButtonFormat, _timeToResendPossible);
            RestartResendCoroutine();

            args.MoveNextFailed += OnMoveNextFailed;
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _sendVerificationCodeButton.onClick.RemoveListener(ResendVerificationCodeButtonClick);
            OpenPageArgs.MoveNextFailed -= OnMoveNextFailed;
            
            ToggleLoadingAnimation(false);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ToggleLoadingAnimation(bool active)
        {
            _loadingIndicator.SetActive(active);
        }

        private void OnValueChanged(string text)
        {
            var isCorrectLength = text.Length == VERIFICATION_CODE_LENGTH;
            _continueButton.SetActive(isCorrectLength);
            _sendVerificationCodeButton.SetActive(!isCorrectLength);
            OpenPageArgs.OnValueChanged?.Invoke(text);
        }
        
        private void ResendVerificationCodeButtonClick()
        {
            SendVerificationCode();
            _timeToResendPossible = WAIT_TIME_TO_RESEND;
            RestartResendCoroutine();
        }

        private IEnumerator ResendCounter()
        {
            _sendVerificationCodeButton.interactable = false;
            while (_timeToResendPossible > 0)
            {
                yield return new WaitForSeconds(1);
                _timeToResendPossible--;
                _sendVerificationCodeText.text = string.Format(_onBoardingLocalization.ResendInButtonFormat, _timeToResendPossible);
                yield return null;
            }

            _sendVerificationCodeButton.interactable = true;
            _sendVerificationCodeText.text = _onBoardingLocalization.ResendCodeButton;
        }

        private void SendVerificationCode()
        {
            OpenPageArgs.NewVerificationCodeRequested?.Invoke();

            _validationText.gameObject.SetActive(false);

            _inputField.ClearText();
            _inputField.Select();
        }

        private void RestartResendCoroutine()
        {
            if (_resendCodeCoroutine != null)
            {
                StopCoroutine(_resendCodeCoroutine);
            }

            _resendCodeCoroutine = StartCoroutine(ResendCounter());
        }

        private void OnContinueButtonClicked()
        {
            ToggleLoadingAnimation(true);

            _continueButton.SetActive(false);
            OpenPageArgs.MoveNextRequested.Invoke();
        }

        private void OnMoveNextFailed()
        {
            ToggleLoadingAnimation(false);
            _validationText.SetActive(true);
            _sendVerificationCodeButton.SetActive(true);
        }

        private void OnBackButtonClicked()
        {
            OpenPageArgs.MoveBackRequested?.Invoke();
        }
    }
}
