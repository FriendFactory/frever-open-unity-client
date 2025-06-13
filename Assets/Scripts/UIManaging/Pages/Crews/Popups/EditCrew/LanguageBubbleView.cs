using System;
using Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews.Popups.EditCrew
{
    internal sealed class LanguageBubbleModel
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public bool Selected { get; private set; }

        public event Action DataUpdated;

        public LanguageBubbleModel(long id, string name, bool selected)
        {
            Id = id;
            Name = name;
            Selected = selected;
        }
        
        public void Update(long? id = null, string name = null, bool? selected = null)
        {
            if (id != null) Id = id.Value;
            if (name != null) Name = name;
            if (selected != null) Selected = selected.Value;
            
            DataUpdated?.Invoke();
        }
    }

    [RequireComponent(typeof(Button))]
    internal sealed class LanguageBubbleView : BaseContextDataView<LanguageBubbleModel>
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        public event Action<long> OnSelected;

        private void OnDisable()
        {
            OnSelected = null;
            ContextData.DataUpdated -= Refresh;
            _button.onClick.RemoveAllListeners();
        }

        protected override void OnInitialized()
        {
            ContextData.DataUpdated += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            _label.text = ContextData.Name;
            _button.interactable = !ContextData.Selected;
            
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (ContextData.Selected) return;
            
            OnSelected?.Invoke(ContextData.Id);
        }
    }
}