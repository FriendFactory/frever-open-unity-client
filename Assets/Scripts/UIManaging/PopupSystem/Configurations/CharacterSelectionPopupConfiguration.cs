using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;

namespace UIManaging.PopupSystem.Configurations
{
    public class CharacterSelectionPopupConfiguration : PopupConfiguration
    {
        public Action<Dictionary<long, CharacterInfo>> SelectionDone { get; }
        public HashSet<long> UniqueIds { get; }
        public HashSet<long> PresetIds { get; }
        public string Header { get; }
        public string HeaderDescription { get; }
        public string ReasonText { get; }
        public Action OnCancelled { get; }
        public long UniverseId { get; set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CharacterSelectionPopupConfiguration(
            HashSet<long> uniqueIds,
            HashSet<long> presetIds,
            string reasonText,
            string header,
            string headerDescription,
            long universeId,
            Action<Dictionary<long, CharacterInfo>> selectionDone, 
            Action onCancelled = null)
        {
            PopupType = PopupType.CharacterSelection;
            UniqueIds = uniqueIds;
            PresetIds = presetIds;
            Header = header;
            HeaderDescription = headerDescription;
            ReasonText = reasonText;
            UniverseId = universeId;
            SelectionDone = selectionDone;
            OnCancelled = onCancelled;
        }
    }
}
