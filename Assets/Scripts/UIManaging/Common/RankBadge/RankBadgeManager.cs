using UnityEngine;

namespace UIManaging.Common.RankBadge
{
    public sealed class RankBadgeManager : MonoBehaviour
    {
        [SerializeField] private RankBadgesConfig _config;

        public Sprite GetBadgeSprite(int id, RankBadgeType type = RankBadgeType.Normal)
        {
            return _config.GetSprite(id, type);
        }
    }
}