using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.EditorsSetting.Settings;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.EditorsCommon.PostRecordEditor;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.Common;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls
{
    internal interface IDeleteEventFeatureControl: IPostRecordEditorFeatureControl
    {
        bool CanDeleteEvent(Event ev);
    }

    [UsedImplicitly]
    internal sealed class DeleteEventFeatureControl : PostRecordEditorFeatureControlBase<EventDeletionSettings>, IDeleteEventFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowDeleting;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.EventDeletion;

        public bool CanDeleteEvent(Event ev)
        {
            return Settings.AllowDeleting;
        }
    }
    
    internal interface ISetLocationSelectionFeatureControl : IPostRecordEditorFeatureControl
    {
    }
    
    [UsedImplicitly]
    internal sealed class SetLocationSelectionFeatureControl : PostRecordEditorFeatureControlBase<SetLocationSettings>, ISetLocationSelectionFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowChangeSetLocation || Settings.AllowVideoUploading || Settings.AllowPhotoUploading;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.SetLocationSelection;
    }

    internal interface IVolumeFeatureControl : IPostRecordEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class VolumeFeatureControl : PostRecordEditorFeatureControlBase<VolumeSettings>, IVolumeFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.VolumeControl;
    }

    internal interface IVfxFeatureControl : IPostRecordEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class VfxFeatureControl : PostRecordEditorFeatureControlBase<VfxSettings>,
                                              IVfxFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.VfxSelection;
    }

    internal interface IBodyAnimationFeatureControl : IPostRecordEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class BodyAnimationFeatureControl :
        PostRecordEditorFeatureControlBase<BodyAnimationSettings>, IBodyAnimationFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.BodyAnimationSelection;
    }
    
    internal interface ICharacterSelectionFeatureControl : IPostRecordEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class CharacterSelectionFeatureControl :
        PostRecordEditorFeatureControlBase<CharacterSettings>, ICharacterSelectionFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.CharacterSelection;
    }

    internal interface ICameraFilterFeatureControl : IPostRecordEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class CameraFilterFeatureControl : PostRecordEditorFeatureControlBase<CameraFilterSettings>, ICameraFilterFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.CameraFilterSelection;
    }
    
    internal interface ICaptionFeatureControl : IPostRecordEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class CaptionFeatureControl : PostRecordEditorFeatureControlBase<CaptionSettings>, ICaptionFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.Captions;
    }
    
    internal interface IVoiceFilterFeatureControl : IPostRecordEditorFeatureControl
    {
        bool IsVoiceFilterDisablingAllowed { get; }
    }

    [UsedImplicitly]
    internal sealed class VoiceFilterFeatureControl : PostRecordEditorFeatureControlBase<VoiceFilterSettings>, IVoiceFilterFeatureControl
    {
        private readonly IBridge _bridge;
        
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.VoiceFilterSelection;

        private readonly ILevelManager _levelManager;

        public bool IsVoiceFilterDisablingAllowed => IsFeatureEnabled && (!_levelManager.CurrentLevel.IsRemix() || IsTargetEventHasUserVoice());

        public VoiceFilterFeatureControl(ILevelManager levelManager, IBridge bridge)
        {
            _levelManager = levelManager;
            _bridge = bridge;
        }

        private bool IsTargetEventHasUserVoice()
        {
            return _levelManager.TargetEvent.GetVoiceTracks().All(x=>x.VoiceOwnerGroupId == _bridge.Profile.GroupId);
        }
    }
    
    internal interface IMusicSelectionFeatureControl : IPostRecordEditorFeatureControl
    {
    }

    [UsedImplicitly]
    internal sealed class MusicSelectionFeatureControl : PostRecordEditorFeatureControlBase<MusicSettings>,
                                                         IMusicSelectionFeatureControl
    {
        public override bool IsFeatureEnabled => Settings.AllowEditing;
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.MusicSelection;
    }

    [UsedImplicitly]
    internal sealed class OutfitFeatureControl : OutfitFeatureControlBase<PostRecordEditorFeatureType>,
                                                 IPostRecordEditorFeatureControl
    {
        public OutfitFeatureControl(LocalUserDataHolder localUserDataHolder, ILevelManager levelManager) : base(localUserDataHolder, levelManager)
        {
        }

        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.OutfitSelection;
    }

    internal interface ICreateNewEventFeatureControl: IPostRecordEditorFeatureControl
    {
        long? TemplateId { get; }
    }

    [UsedImplicitly]
    internal sealed class CreateNewEventFeatureControl : PostRecordEditorFeatureControlBase<EventCreationSettings>, ICreateNewEventFeatureControl
    {
        public override PostRecordEditorFeatureType FeatureType => PostRecordEditorFeatureType.CreateNewEvent;

        public override bool IsFeatureEnabled => Settings.AllowEventCreation;
        public long? TemplateId => Settings.TemplateId;
    }
}