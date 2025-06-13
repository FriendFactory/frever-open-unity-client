using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.StartPack;
using Bridge.Models.ClientServer.StartPack.Metadata;
using JetBrains.Annotations;

namespace Modules.AssetsStoraging.Core
{
    public interface IWardrobeCategoriesProvider
    {
        IEnumerable<WardrobeCategory> GetWardrobeCategories(long gender);
        IEnumerable<WardrobeSubCategory> GetWardrobeSubCategories(long gender);
    }
    
    [UsedImplicitly]
    internal sealed class WardrobeCategoriesProvider: IWardrobeCategoriesProvider
    {
        private readonly IDataFetcher _dataFetcher;
        private WardrobeCategoriesForGender[] _wardrobeCategoriesForGenders;

        public WardrobeCategoriesProvider(IDataFetcher dataFetcher)
        {
            _dataFetcher = dataFetcher;
        }
        
        public IEnumerable<WardrobeCategory> GetWardrobeCategories(long gender)
        {
            return _dataFetcher.WardrobeCategoriesForGenders.First(x => x.GenderId == gender).WardrobeCategories;
        }

        public IEnumerable<WardrobeSubCategory> GetWardrobeSubCategories(long gender)
        {
            return GetWardrobeCategories(gender).SelectMany(x => x.SubCategories);
        }
    }
}