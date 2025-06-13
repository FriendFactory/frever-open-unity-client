using System;
using Bridge.Models.ClientServer.StartPack.Metadata;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Zenject;

namespace Modules.AssetsManaging
{
    [UsedImplicitly]
    internal sealed class VfxCategoryRuntimeAdjuster: IInitializable, IDisposable
    {
        private readonly IDataFetcher _dataFetcher;
        
        public VfxCategoryRuntimeAdjuster(IDataFetcher dataFetcher)
        {
            _dataFetcher = dataFetcher;
        }
        
        public void Initialize()
        {
            if (_dataFetcher.IsStartPackFetched)
            {
                AddWithAnimationsCategory();
            }
            else
            {
                _dataFetcher.OnStartPackFetched += AddWithAnimationsCategory;
            }
        }

        public void Dispose()
        {
            _dataFetcher.OnStartPackFetched -= AddWithAnimationsCategory;
            
            RemoveWithAnimationsCategory();
        }

        private void AddWithAnimationsCategory()
        {
            _dataFetcher.OnStartPackFetched -= AddWithAnimationsCategory;
                
            var vfxCategory = new VfxCategory()
            {
                Id = -1,
                Name = "With animations",
                SortOrder = -1,
            };
                
            _dataFetcher.MetadataStartPack?.VfxCategories?.Add(vfxCategory);
        }
        
        private void RemoveWithAnimationsCategory()
        {
            if (_dataFetcher.MetadataStartPack?.VfxCategories == null) return;
            
            var withAnimationsCategory = _dataFetcher.MetadataStartPack.VfxCategories.Find(x => x.Id == -1);
            
            _dataFetcher.MetadataStartPack.VfxCategories.Remove(withAnimationsCategory);
        }
    }
}