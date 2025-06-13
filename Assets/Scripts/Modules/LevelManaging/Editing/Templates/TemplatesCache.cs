using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge;
using Common;
using Common.ModelsMapping;
using Extensions;
using JetBrains.Annotations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.Templates
{
    public interface ITemplateProvider
    {
        Event GetTemplateEventFromCache(long templateId);
        Task<Event> GetTemplateEvent(long templateId);
    }

    public interface ITemplatesContainer: ITemplateProvider
    {
        void Add(long templateId, Event ev);
        bool Contains(long templateId);
        void Clear();
        Task FetchFromServer(params long[] templates);
    }

    [UsedImplicitly]
    internal sealed class TemplatesContainer: ITemplatesContainer
    {
        private readonly Dictionary<long, Event> _cache = new Dictionary<long, Event>();
        private readonly IBridge _bridge;
        private readonly IMapper _mapper;
        private readonly SnackBarHelper _snackbarHelper;

        public TemplatesContainer(IBridge bridge, IMapper mapper, SnackBarHelper snackbarHelper)
        {
            _bridge = bridge;
            _mapper = mapper;
            _snackbarHelper = snackbarHelper;
        }

        public void Add(long templateId, Event ev)
        {
            if(_cache.ContainsKey(templateId)) return;
            _cache.Add(templateId, ev);
        }
        
        public async Task FetchFromServer(params long[] templates)
        {
            foreach (var templateId in templates)
            {
                if(Contains(templateId)) continue;
                await FetchTemplateEvent(templateId);
            }
        }

        public async Task<Event> GetTemplateEvent(long templateId)
        {
            if (Contains(templateId))
            {
                return GetTemplateEventFromCache(templateId);
            }

            try
            {
                await FetchTemplateEvent(templateId);
                return GetTemplateEventFromCache(templateId);
            }
            catch (Exception exception)
            {
                OnGetTemplateFailed(exception.Message);
            }
            
            return null;
        }

        public bool Contains(long templateId)
        {
            return _cache.ContainsKey(templateId);
        }

        public Event GetTemplateEventFromCache(long templateId)
        {
            if (_cache.TryGetValue(templateId, out var output))
                return output;
            throw new InvalidOperationException($"Templates cache has not cached Template {templateId}");
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private async Task FetchTemplateEvent(long id)
        {
            var eventResp = await _bridge.GetEventForEventTemplate(id);
            if (eventResp.IsError)
            {
                if (eventResp.ErrorMessage.Contains(":403")) return;
                
                throw new InvalidOperationException($"Can't fetch event data for template. Template id: {id}. Reason: {eventResp.ErrorMessage}");
            }

            var levelFullData = eventResp.Model;
            var levelModel = _mapper.Map(levelFullData);
            var templateEvent = levelModel.GetTemplateEvent();
            Add(id, templateEvent);
        }

        private void OnGetTemplateFailed(string message)
        {
            if (message.Contains(Constants.ErrorMessage.ASSET_INACCESSIBLE_IDENTIFIER))
            {
                _snackbarHelper.ShowAssetInaccessibleSnackBar();
            }
            else
            {
                Debug.LogError(message);
            }
        }
    }
}