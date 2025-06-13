using Bridge.Models.ClientServer.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    internal sealed class MyWardrobeListInfoProvider
    {
        private const int EXPIRE_CACHE_SECONDS = 30;
        private readonly IBridge _bridge;
        private readonly List<CachedResponseData> _cachedResponses = new List<CachedResponseData>();

        public MyWardrobeListInfoProvider(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<MyWardrobesListInfo> GetMyWardrobeListInfo(long? categoryId, long? subCategoryId, long? themeCollectionId, long genderId, CancellationToken token)
        {
            var cachedResp = _cachedResponses.FirstOrDefault(x => 
                                                                 x.CategoryId.Compare(categoryId) 
                                                              && x.SubCategoryId.Compare(subCategoryId) 
                                                              && x.ThemeCollectionId.Compare(themeCollectionId) 
                                                              && x.GenderId == genderId);
            if (cachedResp != null)
            {
                if (cachedResp.ExpiredAt > DateTime.UtcNow)
                {
                    return cachedResp.MyWardrobesListInfo;
                }
                _cachedResponses.Remove(cachedResp);
            }

            var result = await _bridge.GetMyWardrobeListInfo(null, genderId, categoryId, subCategoryId, themeCollectionId, token);
            
            if (!result.IsSuccess)
            {
                Debug.LogWarning(result.ErrorMessage);
                return null;
            }
            _cachedResponses.Add(new CachedResponseData()
            {
                CategoryId = categoryId,
                SubCategoryId = subCategoryId,
                ThemeCollectionId = themeCollectionId,
                GenderId = genderId,
                MyWardrobesListInfo = result.Model,
                ExpiredAt = DateTime.UtcNow + TimeSpan.FromSeconds(EXPIRE_CACHE_SECONDS)
            });
            return result.Model;
        }

        public void Clear()
        {
            _cachedResponses.Clear();
        }

        private sealed class CachedResponseData
        {
            public long? CategoryId;
            public long? SubCategoryId;
            public long? ThemeCollectionId;
            public long GenderId;
            public MyWardrobesListInfo MyWardrobesListInfo;
            public DateTime ExpiredAt;
        }
    }
}
