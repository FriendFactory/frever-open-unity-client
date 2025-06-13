using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Services.SelfieAvatar;
using Modules.UserScenarios.Implementation.Common;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    internal interface ICharacterCreationContext: IExitContext
    {
        bool IsNewCharacter { get; set; }
        CharacterFullInfo Character { get; set; }
        CharacterInfo Style { get; set; }
        Race Race { get; set; }
        Gender Gender { get; set; }
        JSONSelfie? JsonSelfie { get; set; }
        Dictionary<CharacterInfo, Sprite> CharacterStyles { get; set; }
        bool AllowBackFromGenderSelection { get; set; }
        CreateMode? SelectedCreateMode { get; set; }
        long? CategoryTypeId { get; set; }
        long? ThemeCollectionId { get; set; }
        bool RaceLocked { get; set; }
        Action OnDisplayed { get; set; }
    }
    
    internal sealed class CharacterCreationContext: ICharacterCreationContext
    {
        public bool IsNewCharacter { get; set; }
        public CharacterFullInfo Character { get; set; }
        public CharacterInfo Style { get; set; }
        public Race Race { get; set; }
        public Gender Gender { get; set; }
        public JSONSelfie? JsonSelfie { get; set; }
        public Dictionary<CharacterInfo, Sprite> CharacterStyles { get; set; }
        public bool AllowBackFromGenderSelection { get; set; }
        public CreateMode? SelectedCreateMode { get; set; }
        public PageId OpenedFromPage { get; set; }
        public long? CategoryTypeId { get; set; }
        public long? ThemeCollectionId { get; set; }
        public bool RaceLocked { get; set; }
        public Action OnDisplayed { get; set; }
    }
}