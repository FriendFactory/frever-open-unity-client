using Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Comments
{
    internal class LoadRepliesButton : BaseContextDataView<CommentRepliesModel>
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        [SerializeField] private Button _loadNextButton;
        [SerializeField] private Button _hideButton;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] private TMP_Text _counterText;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _loadNextButton.onClick.AddListener(OnLoadNextButton);
            _hideButton.onClick.AddListener(OnHideButton);

        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            RefreshState();
            ContextData.NewPageAppended += RefreshState;
            EnableButtons(!ContextData.AwaitingData);
        }
        
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ContextData.NewPageAppended -= RefreshState;
            EnableButtons(true);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnLoadNextButton()
        {
            EnableButtons(false);
            ContextData.LoadNext();
        }

        private void OnHideButton()
        {
            ContextData.Hide();
        }

        private void RefreshState()
        {
            EnableButtons(true);
            _hideButton.gameObject.SetActive(ContextData.LoadedCount > 0);
            _loadNextButton.gameObject.SetActive(!ContextData.IsFullyLoaded);
            if (ContextData.IsFullyLoaded) return;
            _buttonText.text = ContextData.LoadedCount == 0 ? "Show replies" : "Show more";
            _counterText.text = ContextData.LoadedCount == 0 ? $"{ContextData.FullCount}" : $"{ContextData.FullCount - ContextData.LoadedCount}";
        }

        private void EnableButtons(bool isEnabled)
        {
            _hideButton.interactable = isEnabled;
            _loadNextButton.interactable = isEnabled;
        }
    }
}