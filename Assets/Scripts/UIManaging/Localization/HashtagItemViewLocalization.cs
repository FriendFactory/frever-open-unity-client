using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/HashtagItemViewLocalization", fileName = "HashtagItemViewLocalization")]
    public class HashtagItemViewLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _viewsCounterFormat;
        [SerializeField] private LocalizedString _addHashtagTitle;
        
        public string ViewsCounterFormat => _viewsCounterFormat;
        public string AddHashtagTitle => _addHashtagTitle;
    }
}