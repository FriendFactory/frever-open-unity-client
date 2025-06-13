using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Filtering;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Core;
using UnityEngine.Serialization;

namespace Navigation.Args
{
    public class UmaEditorArgs : PageArgs
    {
        public Action BackButtonAction;
        public Action ConfirmButtonAction;
        public Action ClickedOutside;
        public Action LoadingCancellationRequested;
        public Action<CharacterEditorOutput> ConfirmAction;
        public Action SaveOutfitAsFavouriteAction;
        public Action LoadCompleteAction;
        public Action WelcomeGiftReceivedAction;
        public bool IsNewCharacter = true;
        public Gender Gender;
        public CharacterInfo Style;
        public CharacterFullInfo Character;
        public OutfitFullInfo Outfit;
        public CharacterEditorSettings CharacterEditorSettings;
        public TaskFullInfo TaskFullInfo;
        public bool ShowTaskInfo;
        public CharacterEditorConfirmActionType ConfirmActionType;
        public long CategoryTypeId;
        public long? CategoryId;
        public long? SubCategoryId;
        public long? ThemeCollectionId;
        public FilteringSetting DefaultFilteringSetting;
        public bool EnableStoreButton = true;
        public HashSet<long?> OutfitsUsedInLevel;
        public override PageId TargetPage => PageId.UmaEditorNew;
        public long? TaskId => TaskFullInfo?.Id;
        public Action HideLoadingPopup;

        public void OnCancelLoadingRequested()
        {
            LoadingCancellationRequested?.Invoke();
        }

    }

    public struct CharacterEditorOutput
    {
        public CharacterFullInfo Character;
        public OutfitFullInfo Outfit;
    }

    public enum CharacterEditorConfirmActionType
    {
        SaveCharacter,
        SaveOutfitAsAutomatic,
        Onboarding
    }
}