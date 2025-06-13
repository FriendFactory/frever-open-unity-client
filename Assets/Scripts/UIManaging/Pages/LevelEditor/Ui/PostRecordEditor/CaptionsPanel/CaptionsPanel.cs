using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bridge.Models.ClientServer.Level.Full;
using Extensions;
using Modules.ContentModeration;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LocalStorage;
using UIManaging.Common.InputFields;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel.ColorPicking;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class CaptionsPanel : MonoBehaviour
    {
        private const long DEFAULT_FONT_ID = 1;
        private const int CHARACTER_LIMIT = 500;
        
        [Header("Buttons")]
        [SerializeField] private Button _discardButton;
        [SerializeField] private Button _confirmButton;
       
        [Space]
        [SerializeField] private CaptionInputField _inputField;
        [SerializeField] private EditableCaptionView _captionView;
        [SerializeField] private CaptionProjectionToScreenHelper _captionProjectionToScreenHelper;
        [SerializeField] private RectTransform _viewPort;
        [SerializeField] private SizeAdjuster _sizeAdjuster;
        [SerializeField] private DeleteCaptionArea _deleteCaptionArea;
        [SerializeField] private SideBorderHintsControl _borderHintsControl;
        [SerializeField] private List<CaptionStateAndGameObject> _stateGameObjectData;
        [SerializeField] private FontColorsPresenter _fontColorsPresenter;
        [SerializeField] private TextSettingsPanel _textSettingsPanel;
        [SerializeField] private CaptionTransformEditor _captionTransformEditor;
        [SerializeField] private List<RectTransform> _viewPortFitPanels;

        [Inject] private ILevelManager _levelManager;
        [Inject] private InputFieldAdapterFactory _inputFieldAdapterFactory;
        [Inject] private TextContentValidator _textContentValidator;
        [Inject] private CaptionProjectionManager _captionProjectionManager;
        
        private IInputFieldAdapter _inputFieldAdapter;
        private CaptionEditingMode _editingMode;
        private float _initialFontSize;
        private Color _initialColor;
        private CaptionTextAlignment _initialTextAlignment;
        private bool _isNewCaptionCreation;
        private long? _editingCaptionId;
        private CaptionFullInfo _createdCaption;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsShown => gameObject.activeInHierarchy;
        
        private float ScreenToWorldFontSizeRatio => _captionProjectionToScreenHelper.ScreenSpaceFontSizeMultiplier;

        private CaptionEditingMode EditingMode
        {
            get => _editingMode;
            set
            {
                _editingMode = value;
                SetupState(_editingMode);
            }
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action CaptionPanelOpening;
        public event Action Opened;
        public event Action Closed;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public Task Init(Camera cam)
        {
            return _captionProjectionToScreenHelper.Init(cam);
        }

        public void StartNewCaptionCreation()
        {
            _isNewCaptionCreation = true;
            EditingMode = CaptionEditingMode.Text;
            _initialTextAlignment = CaptionTextAlignment.Center;
            SetAlignment(_initialTextAlignment);
            OnShow(null);
        }

        public void StartCaptionTextEditing(long captionId)
        {
            EditingMode = CaptionEditingMode.Text;
            OnShow(captionId);
            var captionModel = GetCaption(captionId);
            _initialTextAlignment = captionModel.TextAlignment;
            SetAlignment(_initialTextAlignment);
            ShowInputField(captionModel.Text);
        }

        public void StartTransformEditing(long captionId)
        {
            var alreadyEditing = EditingMode == CaptionEditingMode.Transform;
            if (alreadyEditing) return;
            EditingMode = CaptionEditingMode.Transform;
            _captionTransformEditor.Completed += OnTransformChangesCompleted;
            _initialTextAlignment = GetCaption(captionId).TextAlignment;
            SetAlignment(_initialTextAlignment);
            _captionTransformEditor.Init(_viewPort);
            _captionTransformEditor.Run();
            OnShow(captionId);
        }

        public void Hide()
        {
            EditingMode = CaptionEditingMode.None;
            gameObject.SetActive(false);
            OnHide();
            Closed?.Invoke();
        }

        public void Cleanup()
        {
            _captionProjectionToScreenHelper.Cleanup();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnShow(long? targetCaptionId)
        {
            if (IsShown) return;
            
            CaptionPanelOpening?.Invoke();
            gameObject.SetActive(true);
            _discardButton.onClick.AddListener(OnCancel);
            _confirmButton.onClick.AddListener(OnConfirm);
            
            _inputFieldAdapter = _inputFieldAdapterFactory.CreateInstance(_inputField);
            _inputFieldAdapter.OnKeyboardStatusChanged += OnKeyboardStatusChanged;

            _inputFieldAdapter.CharacterLimit = CHARACTER_LIMIT;
            _captionView.SetTargetCaptionId(targetCaptionId ?? 0);
            float fontSize;
            if (targetCaptionId.HasValue)
            {
                var captionModel = GetCaption(targetCaptionId.Value);
                fontSize = captionModel.FontSize.ToKilo();
                SelectCaption(targetCaptionId.Value);
            }
            else
            {
                fontSize = Constants.Captions.DEFAULT_FONT_SIZE;
                ShowInputField(string.Empty);
                SetCaptionViewToStartPosition();
            }

            _initialFontSize = fontSize;
            SetupScreenSpaceCaptionView(fontSize);
            foreach (var uiPanel in _viewPortFitPanels)
            {
                _viewPort.CopyProperties(uiPanel);
            }
            
            _captionView.EditButtonClicked += ShowInputField;
            _sizeAdjuster.SizeChanged += OnSizeChanged;
            SetupHints();
            
            var currentColor = targetCaptionId.HasValue ? GetCaption(targetCaptionId.Value).TextColorRgb : Color.white.ToHexRgb();
            _fontColorsPresenter.Initialize(currentColor);
            _fontColorsPresenter.ColorPicked += OnColorPicked;
            var color = ColorExtension.HexToColor(currentColor);
            SetColor(color);
            _initialColor = color;
            Opened?.Invoke();
        }

        private void SetupScreenSpaceCaptionView(float worldFontSize)
        {
            var screenSpaceFont = ConvertToScreenSpaceFontSize(worldFontSize);
            _inputFieldAdapter.FontSize = screenSpaceFont;
            _captionView.SetFontSize(screenSpaceFont);
            _sizeAdjuster.CurrentSize = worldFontSize;
        }

        private void OnHide()
        {
            if (_editingCaptionId.HasValue)
            {
                _captionProjectionManager.SwitchProjection(_editingCaptionId.Value, true);
                _editingCaptionId = null;
            }

            _captionView.Text = null;
            gameObject.SetActive(false);
            _discardButton.onClick.RemoveListener(OnCancel);
            _confirmButton.onClick.RemoveListener(OnConfirm);

            if (_inputFieldAdapter != null)
            {
                _inputFieldAdapter.OnKeyboardStatusChanged -= OnKeyboardStatusChanged;
                _inputFieldAdapter.Dispose();
            }

            _captionView.EditButtonClicked -= ShowInputField;
            _sizeAdjuster.SizeChanged -= OnSizeChanged;
            _fontColorsPresenter.ColorPicked -= OnColorPicked;
            _borderHintsControl.SwitchActiveState(false);
            _textSettingsPanel.AlignmentSelected -= OnAlignmentSelected;
            _textSettingsPanel.Hide();
            _isNewCaptionCreation = false;
            _createdCaption = null;
            _sizeAdjuster.Hide();
            _captionTransformEditor.Completed -= OnTransformChangesCompleted;
        }

        private void OnAlignmentSelected(CaptionTextAlignment alignment)
        {
            SetAlignment(alignment);
            UpdateCaptionModel();
        }

        private void SetAlignment(CaptionTextAlignment alignment)
        {
            _captionView.SetAlignment(alignment);
            _inputField.SetAlignment(alignment.ToTMPTextAlignment());
        }

        private void ShowEditingCaptionView(CaptionFullInfo caption)
        {
            _inputField.gameObject.SetActive(false);

            _captionView.SetActive(true);
            _captionView.Text = caption.Text;
            var screenSpacePos = GetPositionOnScreenOverlayCanvas(caption);
            _captionView.SetTargetCaptionId(caption.Id);
            _captionView.SetNormalizedPosition(screenSpacePos);
            var screenFontSize = ConvertToScreenSpaceFontSize(caption.FontSize.ToKilo());
            _captionView.SetFontSize(screenFontSize);
            _captionView.SetRotation(caption.RotationDegrees.ToKilo());
        }

        private void ShowInputField(string text)
        {
            _inputFieldAdapter.Text = text;
            _inputField.gameObject.SetActive(true);
            _inputFieldAdapter.Select();
            _inputField.SetCaretToTextEnd();

            _captionView.SetActive(false);

            _sizeAdjuster.Show(Constants.Captions.FONT_SIZE_MIN, Constants.Captions.FONT_SIZE_MAX, GetFontSizeWorldSpace());
            EditingMode = CaptionEditingMode.Text;
            _textSettingsPanel.AlignmentSelected += OnAlignmentSelected;
            _textSettingsPanel.Show(_initialTextAlignment);
        }

        private void OnCancel()
        {
            if (_isNewCaptionCreation)
            {
                if (_createdCaption != null)
                {
                    ClearCaption(_createdCaption.Id);
                }
            }
            else
            {
                RestoreInitialFontSize();
                RestoreInitialFontColor();
                RestoreInitialAlignment();
            }

            Hide();
        }

        private void RestoreInitialFontSize()
        {
            if (Math.Abs(_initialFontSize - GetFontSizeWorldSpace()) < 0.001f) return;
            OnSizeChanged(_initialFontSize);
        }

        private void RestoreInitialFontColor()
        {
            SetColor(_initialColor);
        }

        private void RestoreInitialAlignment()
        {
            if (_captionView.Alignment == _initialTextAlignment) return;
            OnAlignmentSelected(_initialTextAlignment);
        }

        private async void OnConfirm()
        {
            if (IsInputTextState())
            {
                if (string.IsNullOrWhiteSpace(_inputFieldAdapter.Text))
                {
                    ClearCaption(_captionView.CaptionId);
                    Hide();
                }
                else
                {
                    var moderationPassed = await _textContentValidator.ValidateTextContent(_inputFieldAdapter.Text);
                    
                    if (!moderationPassed) return;
                    
                    UpdateCaptionModel();
                    Hide();
                }
            }
            else
            {
                UpdateCaptionModel();
                Hide();
            }
        }
        
        private void UpdateCaptionModel()
        {
            var currentCaption = GetTargetCaption();
            var localPos = GetNormalizedLocalPositionOnCameraSpaceCanvas();
            if (_editingMode == CaptionEditingMode.Text)
            {
                var text = _inputField.GetParsedText();
                if (currentCaption == null && !string.IsNullOrEmpty(text))
                {
                    CreateNewCaption(localPos, text);
                }
                else if (currentCaption != null)
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        ClearCaption(currentCaption.Id);
                    }
                    else
                    {
                        currentCaption.Text = text;
                        currentCaption.FontSize = GetFontSizeWorldSpace().ToMilli();
                        currentCaption.TextColorRgb = _captionView.Color.ToHexRgb();
                        currentCaption.TextAlignment = _captionView.Alignment;
                        _levelManager.RefreshCaption(currentCaption);
                    }
                }
            }
            else
            {
                currentCaption.SetNormalizedPosition(localPos);
                currentCaption.RotationDegrees = _captionView.RotationEulerAngle.ToMilli();
                _levelManager.RefreshCaption(currentCaption);
            }
            
            RefreshProjections();
        }

        private void CreateNewCaption(Vector2 localPos, string text)
        {
            var newCaption = new CaptionFullInfo
            {
                Id = LocalStorageManager.GetNextLocalId(nameof(CaptionFullInfo)),
                FontId = DEFAULT_FONT_ID,
                FontSize = GetFontSizeWorldSpace().ToMilli(),
                Text = text,
                RotationDegrees = 0,
                TextColorRgb = _captionView.Color.ToHexRgb(),
                TextAlignment = _captionView.Alignment
            };
            newCaption.SetNormalizedPosition(localPos);
            _captionView.SetTargetCaptionId(newCaption.Id);
            _captionView.SetRotation(newCaption.RotationDegrees.ToKilo());
            _editingCaptionId = newCaption.Id;
            _levelManager.AddCaption(newCaption);
            _levelManager.PauseCaption(newCaption.Id);
            _createdCaption = newCaption;
        }

        private CaptionFullInfo GetTargetCaption()
        {
            return GetCurrentEvent().Caption.FirstOrDefault(x => x.Id == _captionView.CaptionId);
        }

        private Vector2 GetNormalizedLocalPositionOnCameraSpaceCanvas()
        {
            return _captionProjectionToScreenHelper.GetCameraSpaceLocalPositionNormalized();
        }
        
        private void SetCaptionViewToStartPosition()
        {
            _captionView.transform.position = _viewPort.TransformPoint(_viewPort.rect.center);
        }

        private Vector2 GetPositionOnScreenOverlayCanvas(CaptionFullInfo model)
        {
            return _captionProjectionToScreenHelper.ConvertLocalNormalizedToScreenSpacePositionsNormalized(model.GetNormalizedPosition());
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
        private void OnKeyboardStatusChanged(KeyboardStatus status)
        {
            switch (status)
            {
                case KeyboardStatus.Canceled:
        #if UNITY_IOS
                    // Workaround for TMP_InputField issue when canceling iOS keyboard
                    CoroutineSource.Instance.ExecuteWithFrameDelay(OnCancel);
                    break;
        #endif
                case KeyboardStatus.Done:
                    OnConfirm();
                    break;
            }
        }

        private Event GetCurrentEvent()
        {
            return _levelManager.TargetEvent;
        }

        private CaptionFullInfo GetCaption(long id)
        {
            return GetCurrentEvent().Caption.First(x=>x.Id == id);
        }

        private bool IsInputTextState()
        {
            return _inputField.gameObject.activeInHierarchy;
        }
        
        private void ClearCaption(long captionId)
        {
            _levelManager.RemoveCaption(captionId);
        }

        private void SelectCaption(long captionId)
        {
            if (captionId == _editingCaptionId) return;
            
            if (_editingCaptionId.HasValue)
            {
                UpdateCaptionModel();
                _levelManager.ResumeCaption(_editingCaptionId.Value);
                _captionProjectionManager.SwitchProjection(_editingCaptionId.Value, true);
            }

            _editingCaptionId = captionId;
            _levelManager.PauseCaption(captionId);
            var caption = GetCaption(captionId);
            ShowEditingCaptionView(caption);
            RefreshProjections();
            _captionProjectionManager.SwitchProjection(captionId, false);
        }

        private void RefreshProjections()
        {
            _captionProjectionManager.SetupCaptionsProjection(_levelManager.TargetEvent.Caption);
        }
        
        private void OnSizeChanged(float size)
        {
            var screenSpaceSize = ConvertToScreenSpaceFontSize(size);
            _captionView.SetFontSize(screenSpaceSize);
            _inputFieldAdapter.FontSize = screenSpaceSize;
            _inputField.Refresh();
            UpdateCaptionModel();
        }

        private float ConvertToScreenSpaceFontSize(float worldFontSize)
        {
            return worldFontSize / ScreenToWorldFontSizeRatio;
        }
        
        private float GetFontSizeWorldSpace()
        {
            var screenSpaceSize = _captionView.FontSize;
            return screenSpaceSize * ScreenToWorldFontSizeRatio;
        }
        
        private void OnTransformChangesCompleted()
        {
            var shouldDeleteCaption = _deleteCaptionArea.IsCaptionInsideArea;
            if (shouldDeleteCaption)
            {
                ClearCaption(_captionView.CaptionId);
            }
            else
            {
                UpdateCaptionModel();
            }

            Hide();
        }

        private void SetupHints()
        {
            _borderHintsControl.Init(_viewPort);
            _borderHintsControl.SwitchActiveState(EditingMode == CaptionEditingMode.Transform);
        }

        private void OnColorPicked(Color color)
        {
            SetColor(color);
            UpdateCaptionModel();
        }
        
        private void SetColor(Color color)
        {
            _captionView.SetColor(color);
            _inputField.TextRenderer.Color = color;
        }
        
        private void SetupState(CaptionEditingMode editingMode)
        {
            foreach (var data in _stateGameObjectData)
            {
                var shouldBeActive = data.ActiveInStates.Any(x => (x & editingMode) != 0);
                data.Target.SetActive(shouldBeActive);
            }
        }
        
        [Serializable] 
        private struct CaptionStateAndGameObject
        {
            public GameObject Target;
            public CaptionEditingMode[] ActiveInStates;
        }
       
        private enum CaptionEditingMode
        {
            None,
            Text,
            Transform
        }
    }
}