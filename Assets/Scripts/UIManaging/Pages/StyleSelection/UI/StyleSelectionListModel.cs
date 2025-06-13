using System.Collections.Generic;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.StyleSelection.UI
{
    internal sealed class StyleSelectionListModel
    {
        public CharacterInfo[] StylePresets { get; }
        public IReadOnlyDictionary<CharacterInfo, Sprite> PresetThumbnails { get; }
        public float CellSize { get; }
        public bool ContainsSelfieButton { get; }

        public StyleSelectionListModel(CharacterInfo[] presets, IReadOnlyDictionary<CharacterInfo, Sprite> presetThumbnails, float cellSize, bool containsSelfieButton = false)
        {
            StylePresets = presets;
            PresetThumbnails = presetThumbnails;
            CellSize = cellSize;
            ContainsSelfieButton = containsSelfieButton;
        }
    }
}
