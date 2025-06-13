using System.Collections.Generic;
using Modules.WardrobeManaging;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    /// <summary>
    /// Used as a temporary solution for data that should be returned from the backend
    /// </summary>
    
    [CreateAssetMenu(fileName = "WardrobePanelExtraSettings", menuName = "Friend Factory/Wardrobe/Wardrobe Panel Settings", order = 2)]
    public sealed class WardrobePanelExtraSettings: ScriptableObject
    {
        public List<WardrobeCategoryData> WardrobeCategoryDatas;
    }
}