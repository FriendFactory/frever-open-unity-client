using Abstract;
using AdvancedInputFieldPlugin;
using UIManaging.Common.InputFields;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.RegistrationInputFields
{
    public abstract class
        SpecializedInputFieldBase : BaseContextDataView<SpecializedInputFieldModel>
    {
        #if ADVANCEDINPUTFIELD_TEXTMESHPRO
            [SerializeField] protected AdvancedInputField _inputField;
        #else
            [SerializeField] private TMP_InputField _inputField;
        #endif
        

        private PopupManager _popupManager;
        private InputFieldAdapterFactory _inputFieldAdapterFactory;

        private IInputFieldAdapter _inputFieldAdapter;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public abstract SpecializationType Type { get; }
        protected IInputFieldAdapter InputField => _inputFieldAdapter;
        protected virtual bool OpenKeyboardOnDisplay => false;
        protected virtual bool AllowPaste => false;

        [Inject]
        private void Construct(PopupManager popupManager, InputFieldAdapterFactory inputFieldAdapterFactory)
        {
            _popupManager = popupManager;
            _inputFieldAdapterFactory = inputFieldAdapterFactory;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public virtual void Display()
        {
            gameObject.SetActive(true);
            
            if (OpenKeyboardOnDisplay)
            {
                Select();
            }
        }

        public void Select()
        {
            _inputFieldAdapter.Select();
        }

        public void SetText(string text)
        {
            _inputFieldAdapter.SetTextWithoutNotify(text);
        }

        public void ClearText()
        {
            _inputFieldAdapter.Text = string.Empty;
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual string ApplyTextAlterations()
        {
            return _inputFieldAdapter.Text;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            SetupInputFieldAdapter();
            if(AllowPaste) SetupPastePopup();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            CleanUpInputFieldAdapter();
        }

        protected virtual void SetInitialText(string text)
        {
            SetText(text ?? string.Empty);
        }
        
        protected virtual void OnValueChanged(string text)
        {
            ContextData.OnValueChanged?.Invoke(text);
        }
        
        private void OnKeyboardStatusChanged(KeyboardStatus keyboardStatus)
        {
            ContextData.OnKeyboardStatusChanged?.Invoke(keyboardStatus);
            if(keyboardStatus != KeyboardStatus.Done) return;
            ContextData.OnKeyboardSubmit?.Invoke();
        }


        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetupPastePopup()
        {
            var config = new PastePopupConfiguration
            {
                PopupType = PopupType.PastePopup,
                InputField = _inputFieldAdapter
            };
            _popupManager.SetupPopup(config);
        }

        private void SetupInputFieldAdapter()
        {
            _inputFieldAdapter = _inputFieldAdapterFactory.CreateInstance(_inputField);
            _inputFieldAdapter.CharacterLimit = ContextData.CharacterLimit;
            _inputFieldAdapter.PlaceholderText = ContextData.PlaceHolderText;
            _inputFieldAdapter.InputType = ContextData.InputType;
            _inputFieldAdapter.ContentType = ContextData.ContentType;
            
            SetInitialText(ContextData.InitialText);
            
            _inputFieldAdapter.OnValueChanged += OnValueChanged;
            _inputFieldAdapter.OnKeyboardStatusChanged += OnKeyboardStatusChanged;
        }
        
        private void CleanUpInputFieldAdapter()
        {
            if (_inputFieldAdapter == null) return;
            _inputFieldAdapter.OnValueChanged -= OnValueChanged;
            _inputFieldAdapter.OnKeyboardStatusChanged -= OnKeyboardStatusChanged;
            _inputFieldAdapter.Dispose();
        }
    }
}