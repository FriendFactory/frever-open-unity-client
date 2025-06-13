using System.Runtime.CompilerServices;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Zenject;

[assembly: InternalsVisibleTo("Installers")]
namespace UIManaging.Pages.CreatorScore
{
    [UsedImplicitly]
    public class CreatorScoreHelper
    {
        [Inject] private IDataFetcher _dataFetcher;
        
        public int GetBadgeRank(int creatorScoreLevel)
        {
            var rank = 0;

            var allLevels = _dataFetcher.MetadataStartPack.CreatorBadges;

            if (allLevels.IsNullOrEmpty()) return rank;
            
            foreach (var level in allLevels)
            {
                if (creatorScoreLevel < level.Level)
                {
                    break;
                }
                
                if (!string.IsNullOrEmpty(level.Milestone))
                {
                    rank++;
                }
            }

            return rank;
        }
        
    }
}