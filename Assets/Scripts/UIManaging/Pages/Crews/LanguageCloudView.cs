using System;
using System.Collections.Generic;
using Abstract;
using Bridge.Models.ClientServer;
using UIManaging.Pages.Crews.Popups.EditCrew;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews
{
    internal sealed class LanguageCloudModel
    {
        public readonly List<LanguageBubbleModel> Models;

        public LanguageCloudModel(LanguageInfo[] languages, long currentLanguage = -1)
        {
            Models = new List<LanguageBubbleModel>();
            
            foreach (var language in languages)
            {
                var model = new LanguageBubbleModel(language.Id, language.Name, currentLanguage == language.Id);
                Models.Add(model);
            }
        }
    }

    internal sealed class LanguageCloudView : BaseContextDataView<LanguageCloudModel>
    {
        [SerializeField] private Transform _languagesParent;
        [SerializeField] private LanguageBubbleView _languageBubble;

        private Dictionary<long, LanguageBubbleView> _views = new Dictionary<long, LanguageBubbleView>();

        public event Action<long> LanguageSelected;

        private void OnDisable()
        {
            CleanUp();
        }

        public void SetLanguage(long id)
        {
            OnLanguageSelected(id);
        }

        protected override void OnInitialized()
        {
            Refresh();
        }

        private void Refresh()
        {
            foreach (var model in ContextData.Models)
            {
                if (_views.TryGetValue(model.Id, out var view))
                {
                    view.Initialize(model);
                    view.OnSelected += OnLanguageSelected;
                    
                    continue;
                }
                
                InstantiateNewBubble(model);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }
        
        
        private void InstantiateNewBubble(LanguageBubbleModel model)
        {
            var bubble = Instantiate(_languageBubble, _languagesParent);
            
            bubble.Initialize(model);
            bubble.OnSelected += OnLanguageSelected;
            _views.Add(model.Id, bubble);
        }

        private void OnLanguageSelected(long id)
        {
            foreach (var m in ContextData.Models)
            {
                var selected = m.Id == id;
                m.Update(selected: selected);
            }
            
            LanguageSelected?.Invoke(id);
        }
    }
}