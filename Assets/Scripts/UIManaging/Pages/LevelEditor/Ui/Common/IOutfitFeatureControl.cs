using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.EditorsSetting.Settings;
using Extensions;
using Models;
using Modules.EditorsCommon;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.UsersManagement;

namespace UIManaging.Pages.LevelEditor.Ui.Common
{
    internal interface IOutfitFeatureControl: IFeatureControl
    {
        bool CreationOutfitAllowed { get; }
        bool CanChangeOutfit(CharacterFullInfo character);
        bool CanChangeOutfit(CharacterInfo character);
        
        bool CanChangeOutfit(CharacterFullInfo character, ref string reason);
        bool CanChangeOutfit(CharacterInfo character, ref string reason);
        
        bool CanChangeOutfitForTargetCharacter();
        bool CanChangeOutfitForTargetCharacter(ref string reason);
    }
    
    
    internal abstract class OutfitFeatureControlBase<TFeatureType>: FeatureControlBase<OutfitSettings, TFeatureType>, IOutfitFeatureControl
        where TFeatureType : Enum
    {
        private readonly LocalUserDataHolder _localUserDataHolder;
        private readonly ILevelManager _levelManager;
        public bool CreationOutfitAllowed => Settings.AllowCreateNew;

        public OutfitFeatureControlBase(LocalUserDataHolder localUserDataHolder, ILevelManager levelManager)
        {
            _localUserDataHolder = localUserDataHolder;
            _levelManager = levelManager;
        }

        public override bool IsFeatureEnabled => Settings.AllowEditing;

        public bool CanChangeOutfit(CharacterFullInfo character)
        {
            string reason = null;
            return CanChangeOutfit(character.GroupId, character.IsFreverStar, false, ref reason);
        }

        public bool CanChangeOutfit(CharacterInfo character)
        {
            string reason = null;
            return CanChangeOutfit(character.GroupId, character.IsFreverStar, false, ref reason);
        }

        public bool CanChangeOutfit(CharacterFullInfo character, ref string reason)
        {
            return CanChangeOutfit(character.GroupId, character.IsFreverStar, true, ref reason);
        }

        public bool CanChangeOutfit(CharacterInfo character, ref string reason)
        {
            return CanChangeOutfit(character.GroupId, character.IsFreverStar, true, ref reason);
        }

        public bool CanChangeOutfitForTargetCharacter()
        {
            string reason = null;
            return CanChangeOutfitForTargetCharacter(ref reason);
        }

        public bool CanChangeOutfitForTargetCharacter(ref string reason)
        {
            var isGroupFocus = _levelManager.EditingCharacterSequenceNumber == -1;
            if (isGroupFocus)
            {
                reason = $"Select one of your characters to change";
                return false;
            }
            
            var targetCharacter = _levelManager.TargetEvent.GetCharacterControllerBySequenceNumber(_levelManager.EditingCharacterSequenceNumber).Character;
            return CanChangeOutfit(targetCharacter, ref reason);
        }

        private bool CanChangeOutfit(long characterOwnerGroupId, bool isFreverStar, bool providerReason, ref string reason)
        {
            if (!IsFeatureEnabled)
            {
                if (providerReason)
                {
                    reason = $"Can't change outfit";
                }
                return false;
            }

            if (isFreverStar)
            {
                if(Settings.AllowForFreverStars) return true;

                if (providerReason) reason = $"Can't change Frever Star's outfit";
                return false;
            }
            var isOwnCharacter = _localUserDataHolder.GroupId == characterOwnerGroupId;
            if (isOwnCharacter)
            {
                if(Settings.AllowForOwnCharacters) return true;

                if (providerReason) reason = $"Can't change yours outfit";
                return false;
            }

            if (Settings.AllowForFriendCharacters) return true;

            if (providerReason) reason = $"Can't change friend's outfit";
            return false;
        }
    }
}