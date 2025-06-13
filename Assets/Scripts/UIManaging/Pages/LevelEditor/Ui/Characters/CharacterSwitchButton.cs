using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    public class CharacterSwitchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler,
                                            IBeginDragHandler, IEndDragHandler
    {
        private const float GRACE_PERIOD_TIME = 0.2f;

        [SerializeField] protected Image _thumbnail;

        private bool _isPostRecordEditor;

        protected ILevelManager LevelManager;

        private IBridge _bridge;
        private ICameraSystem _cameraSystem;
        private AmplitudeManager _amplitudeManager;

        private Event _targetEvent;

        private RectTransform _rectTransform;

        private ScrollRect _scrollRectParent;

        private UnityAction _onBeginDrag;
        private UnityAction _onEndDrag;

        private int _currentPointerId;
        private bool _isGracePeriodRunning;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action ButtonClicked;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }

        public int TargetSequenceNumber { get; private set; }
        public bool IsSelected { get; set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager, IBridge bridge, ICameraSystem cameraSystem, AmplitudeManager amplitudeManager)
        {
            LevelManager = levelManager;
            _bridge = bridge;
            _cameraSystem = cameraSystem;
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(int targetSequenceNumber, bool isPostRecordEditor)
        {
            TargetSequenceNumber = targetSequenceNumber;
            _isPostRecordEditor = isPostRecordEditor;
        }

        public virtual async void RefreshThumbnail()
        {
            var characterController = LevelManager.TargetEvent.GetCharacterController(TargetSequenceNumber);
            if (characterController == null) return;

            var character = characterController.Character;
            var thumbnailFile = character.Files.First(x=>x.Resolution == Resolution._128x128);
            var result = await _bridge.GetCharacterThumbnailAsync(character.Id, thumbnailFile);
            if(result.IsRequestCanceled) return;
            
            if (result.IsSuccess)
            {
                var thumbnail = (result.Object as Texture2D).ToSprite();
                _thumbnail.sprite = thumbnail;
            }
            else
            {
                Debug.LogWarning(result.ErrorMessage);
            }
        }

        public void OnClick()
        {
            if (IsSelected) return;

            SetTargetSequenceNumber(TargetSequenceNumber);

            if (!_isPostRecordEditor)
            {
                SwitchCharacterCameraFocus();
            }
            
            ButtonClicked?.Invoke();
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        public void SetInteractable(bool interactable)
        {
            _thumbnail.raycastTarget = interactable;
        }

        public void SetupScrollRect(ScrollRect scrollRect)
        {
            _scrollRectParent = scrollRect;
        }

        public void SetupOnBeginDragEvent(UnityAction onBeginDrag)
        {
            _onBeginDrag = onBeginDrag;
        }

        public void SetupOnEndDragEvent(UnityAction onEndDrag)
        {
            _onEndDrag = onEndDrag;
        }

        //---------------------------------------------------------------------
        // IPointerHandler
        //---------------------------------------------------------------------

        public void OnPointerDown(PointerEventData eventData)
        {
            _currentPointerId = eventData.pointerId;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_currentPointerId != eventData.pointerId) return;
            StopGracePeriodIfRunning();

            if (eventData.dragging) return;
            OnClick();
        }

        //---------------------------------------------------------------------
        // IDragHandler
        //---------------------------------------------------------------------

        public void OnDrag(PointerEventData eventData)
        {
            if (!_scrollRectParent.enabled) return;

            _scrollRectParent.OnDrag(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_scrollRectParent.enabled) return;

            _scrollRectParent.OnBeginDrag(eventData);
            _onBeginDrag?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_scrollRectParent.enabled) return;

            _scrollRectParent.OnEndDrag(eventData);
            _onEndDrag?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void StopGracePeriodIfRunning()
        {
            if (!_isGracePeriodRunning) return;

            StopCoroutine(ScrollGracePeriod());
            _isGracePeriodRunning = false;
        }

        private IEnumerator ScrollGracePeriod()
        {
            _isGracePeriodRunning = true;
            yield return new WaitForSeconds(GRACE_PERIOD_TIME);
            _isGracePeriodRunning = false;
        }

        private void SetTargetSequenceNumber(int sequenceNumber)
        {
            LevelManager.EditingCharacterSequenceNumber = sequenceNumber;
            if (!_isPostRecordEditor)
            {
                //also change target event character
                LevelManager.TargetCharacterSequenceNumber = sequenceNumber;
                
                var focusCharacterChangedMetaData = new Dictionary<string, object> {[AmplitudeEventConstants.EventProperties.CHARACTER_SEQUENCE_NUMBER] = sequenceNumber};
                _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CHARACTER_FOCUS_CHANGED, focusCharacterChangedMetaData);
            }
        }

        private void SwitchCharacterCameraFocus()
        {
            var currentCharacter = LevelManager.TargetCharacterAsset;
            var lookAt = currentCharacter != null ? currentCharacter.LookAtBoneGameObject : _cameraSystem.GetCinemachineLookAtTargetGroup().gameObject;
            var follow = currentCharacter != null ? currentCharacter.GameObject : _cameraSystem.GetCinemachineFollowTargetGroup().gameObject;

            _cameraSystem.SetTargets(lookAt, follow, false);
        }

    }
}