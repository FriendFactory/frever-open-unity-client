using System.Collections.Generic;
using System.Linq;
using Navigation.Core;
using UnityEngine;

namespace TipsManagment
{
    [CreateAssetMenu(fileName = "TutorialConfig.asset", menuName = "Friend Factory/Configs/Tutorial Config", order = 4)]
    public class TutorialConfig : ScriptableObject
    {
        public List<TipPreset> TipPresets;

        public List<TipPreset> GetPageTips(PageId pageId)
        {
            var pageTipPresets = TipPresets.Where(x => x.Page == pageId).ToList();
            return pageTipPresets;
        }
        
        public List<TipPreset> GetDirectTips(TipId id)
        {
            var tipPresets = TipPresets.Where(x => x.Settings.Id == id).ToList();
            return tipPresets;
        }
    }
}