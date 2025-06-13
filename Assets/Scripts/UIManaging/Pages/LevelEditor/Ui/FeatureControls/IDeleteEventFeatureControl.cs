using System.Linq;
using Bridge.Models.ClientServer.EditorsSetting.Settings;
using JetBrains.Annotations;
using Models;
using Modules.EditorsCommon.LevelEditor;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal interface IDeleteEventFeatureControl: ILevelEditorFeatureControl
    {
        bool CanDeleteEvent(Event ev);
    }

    [UsedImplicitly]
    internal sealed class DeleteEventFeatureControl : LevelEditorFeatureControlBase<EventDeletionSettings>, IDeleteEventFeatureControl
    {
        private readonly LevelEditorPageModel _pageModel;

        public DeleteEventFeatureControl(LevelEditorPageModel pageModel)
        {
            _pageModel = pageModel;
        }

        public override bool IsFeatureEnabled => Settings.AllowDeleting;
        public override LevelEditorFeatureType FeatureType => LevelEditorFeatureType.EventDeletion;

        private bool OnlyNewEventsDeletionAllowed => _pageModel.OpeningPageArgs.NewEventsDeletionOnly;
        private Level OriginalLevel => _pageModel.OpeningPageArgs.LevelData;
        
        public bool CanDeleteEvent(Event ev)
        {
            if (!Settings.AllowDeleting) return false;

            if (!OnlyNewEventsDeletionAllowed) return true;
            
            return IsNewRecordedEvent(ev);
        }

        private bool IsNewRecordedEvent(Event ev)
        {
            return OriginalLevel == null || OriginalLevel.Event.All(x => x.Id != ev.Id);
        }
    }
}