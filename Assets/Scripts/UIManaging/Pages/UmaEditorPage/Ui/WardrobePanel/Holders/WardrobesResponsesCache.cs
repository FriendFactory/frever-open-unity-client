using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.ClientServer.Assets.Wardrobes;
using Bridge.Models.ClientServer.Assets;
using Bridge.Results;
using JetBrains.Annotations;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    [UsedImplicitly]
    internal sealed class WardrobesResponsesCache
    {
        private readonly TimeSpan _expirationTime = TimeSpan.FromMinutes(5); 
        private readonly List<CachedResponseData> _responses = new List<CachedResponseData>();

        public bool TryGetFromCache(RequestType requestType, IWardrobeFilter wardrobeFilter, out ArrayResult<WardrobeShortInfo> wardrobes)
        {
            ClearExpiredResponses();
            
            wardrobes = _responses.FirstOrDefault(x => x.RequestType == requestType && x.WardrobeFilter.Equals(wardrobeFilter))?.Wardrobes;
            return wardrobes != null;
        } 
        
        public void AddResponse(RequestType requestType, IWardrobeFilter wardrobeFilter, ArrayResult<WardrobeShortInfo> wardrobes)
        {
            var responseData = new CachedResponseData
            {
                RequestType = requestType,
                WardrobeFilter = wardrobeFilter,
                Wardrobes = wardrobes,
                ExpirationDate = DateTime.Now + _expirationTime
            };
            _responses.Add(responseData);
        }

        public void Invalidate()
        {
            _responses.Clear();
        }

        private void ClearExpiredResponses()
        {
            var expired = _responses.Where(x => x.ExpirationDate <= DateTime.Now).ToArray();
            foreach (var data in expired)
            {
                _responses.Remove(data);
            }
        }
    }
    
    internal class CachedResponseData
    {
        public RequestType RequestType;
        public IWardrobeFilter WardrobeFilter;
        public ArrayResult<WardrobeShortInfo> Wardrobes;
        public DateTime ExpirationDate;
    }
}