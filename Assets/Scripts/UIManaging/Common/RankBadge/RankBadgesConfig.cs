using UnityEngine;

namespace UIManaging.Common.RankBadge
{
	[CreateAssetMenu(fileName = "RankBadgesConfig.asset", menuName = "Friend Factory/Configs/Rank Badges Config", order = 4)]
	public class RankBadgesConfig : ScriptableObject
	{
		[SerializeField] private Sprite[] _smallSprites;
		[SerializeField] private Sprite[] _normalSprites;
		[SerializeField] private Sprite[] _homepageSprites;
		
		public Sprite GetSprite(int id, RankBadgeType type = RankBadgeType.Normal)
		{
			Sprite[] sprites;
			switch (type)
			{
				case RankBadgeType.Homepage:
					sprites = _homepageSprites;
					break;
				case RankBadgeType.Normal:
					sprites = _normalSprites;
					break;
				case RankBadgeType.Small:
					sprites = _smallSprites;
					break;
				default:
					Debug.LogError($"There is no such kind of sprites type={type}.");
					return null;
			}	
			
			if (id >= sprites.Length)
			{
				Debug.LogError($"The badge id={id} has no corresponding badge image in the {type} collection");
				return null;
			}

			return sprites[id];
		}
	}
}