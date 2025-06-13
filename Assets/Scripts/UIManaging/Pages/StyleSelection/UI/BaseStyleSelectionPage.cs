using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Amplitude;
using Modules.InputHandling;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.StyleSelection.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.UmaEditorPage.Ui.Stages
{
    internal abstract class BaseStyleSelectionPage<T> : GenericPage<T> where T : PageArgs, IStyleSelectionArgs
    {
        [SerializeField] protected StyleSelectionList _styleSelectionList;
        [SerializeField] private Button _backButton;
        [SerializeField] protected Button _confirmButton;

        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private IInputManager _inputManager;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
        }

        //---------------------------------------------------------------------
        // Page
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override void OnDisplayStart(T args)
        {
            base.OnDisplayStart(args);
            if (!args.SelectedStyle.HasValue)
            {
                _styleSelectionList.Reset();
            }
            else
            {
                _styleSelectionList.SetSelected(args.SelectedStyle.Value);
            }
            _inputManager.Enable(true);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _inputManager.Enable(false);
        }

        //---------------------------------------------------------------------
        // UI Callbacks
        //---------------------------------------------------------------------

        protected virtual void OnConfirmButtonClicked()
        {
#if UNITY_EDITOR
            if (_styleSelectionList.SelectedStyle == null) return;
#endif
            var styleMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.CHARACTER_ID] = _styleSelectionList.SelectedStyle.Id
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.STYLE_SELECTED, styleMetaData);
        }

        protected virtual void OnBackButtonClicked()
        {
            throw new NotImplementedException("The method has to be overriden!");
        }

        //---------------------------------------------------------------------
        // Other
        //---------------------------------------------------------------------

        protected virtual void FillStyles(IReadOnlyDictionary<CharacterInfo, Sprite> thumbnails)
        {
            if (thumbnails is null)
            {
                return;
            }
            var stylePresets = thumbnails.Keys;
            var cellSize = 850.0f;
            var styleListModel = new StyleSelectionListModel(stylePresets.ToArray(), thumbnails, cellSize);
            _styleSelectionList.Initialize(styleListModel);
        }

        protected void ClearStyleSelectionList()
        {
            _styleSelectionList.Clear();
        }
    }
}