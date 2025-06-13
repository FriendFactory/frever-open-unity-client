using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Authorization.Models;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Services.SelfieAvatar;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using Modules.UserScenarios.Implementation.LevelCreation;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace Modules.UserScenarios.Implementation.Onboarding
{
    internal sealed class OnboardingContext: ICharacterCreationContext, ISignupContext
    {
        public Serializable SerializableContext { get; set; }
        public Action OnDisplayed { get; set; }

        public bool IsNewCharacter
        {
            get => SerializableContext.IsNewCharacter;
            set => SerializableContext.IsNewCharacter = value;
        }
        
        public CharacterFullInfo Character
        {
            get => SerializableContext.Character;
            set => SerializableContext.Character = value;
        }
        
        public CharacterInfo Style 
        {
            get => SerializableContext.Style;
            set => SerializableContext.Style = value;
        }

        public Race Race
        {
            get => SerializableContext.Race;
            set => SerializableContext.Race = value;
        }

        public Gender Gender 
        {
            get => SerializableContext.Gender;
            set => SerializableContext.Gender = value;
        }
        
        public JSONSelfie? JsonSelfie 
        {
            get => SerializableContext.JsonSelfie;
            set => SerializableContext.JsonSelfie = value;
        }

        private Dictionary<CharacterInfo, Sprite> _characterStyles;
        
        public Dictionary<CharacterInfo, Sprite> CharacterStyles
        {
            get => _characterStyles;
            set
            {
                _characterStyles = value;
                SerializableContext.CharacterStyles = _characterStyles.Keys.ToList();
            } }
        
        public bool AllowBackFromGenderSelection 
        {
            get => SerializableContext.AllowBackFromGenderSelection;
            set => SerializableContext.AllowBackFromGenderSelection = value;
        }

        public CreateMode? SelectedCreateMode
        {
            get => SerializableContext.SelectedCreateMode;
            set => SerializableContext.SelectedCreateMode = value;
        }
        
        public long? CategoryTypeId { get; set; }
        public long? ThemeCollectionId { get; set; }
        public bool RaceLocked { get; set; }

        public string NavigationMessage 
        {
            get => SerializableContext.NavigationMessage;
            set => SerializableContext.NavigationMessage = value;
        }
        
        public PageId OpenedFromPage 
        {
            get => SerializableContext.OpenedFromPage;
            set => SerializableContext.OpenedFromPage = value;
        }
        public TaskFullInfo Task 
        {
            get => SerializableContext.Task;
            set => SerializableContext.Task = value;
        }

        public LevelEditorContext LevelEditor 
        {
            get => SerializableContext.LevelEditor ?? new LevelEditorContext();
            set => SerializableContext.LevelEditor = value;
        }

        public CharacterEditorContext CharacterEditor 
        {
            get => SerializableContext.CharacterEditor ?? new CharacterEditorContext();
            set => SerializableContext.CharacterEditor = value;
        }
        
        public CharacterSelectionContext CharacterSelection 
        {
            get => SerializableContext.CharacterSelection ?? new CharacterSelectionContext();
            set => SerializableContext.CharacterSelection = value;
        }
        
        public ICredentials Credentials { get; set; }
        
        public long CategoryId  {
            get => SerializableContext.CategoryId;
            set => SerializableContext.CategoryId = value;
        }

        public class Serializable
        {
            public ScenarioState CurrentState = ScenarioState.SignUpBirthday;
            
            public bool IsNewCharacter;
            public CharacterFullInfo Character;
            public CharacterInfo Style;
            public Race Race;
            public Gender Gender;
            public JSONSelfie? JsonSelfie;
            public List<CharacterInfo> CharacterStyles;
            public bool AllowBackFromGenderSelection;
            public CreateMode? SelectedCreateMode;
            public long CategoryId;
            
            public string NavigationMessage;

            public PageId OpenedFromPage;
            public TaskFullInfo Task;
            public bool VideoUploaded;

            public LevelEditorContext LevelEditor = new LevelEditorContext();
            public PostRecordEditorContext PostRecordEditor = new PostRecordEditorContext();
            public CharacterEditorContext CharacterEditor = new CharacterEditorContext();
            public CharacterSelectionContext CharacterSelection = new CharacterSelectionContext();
            public PublishContext PublishContext = new PublishContext();
        }
    }
}