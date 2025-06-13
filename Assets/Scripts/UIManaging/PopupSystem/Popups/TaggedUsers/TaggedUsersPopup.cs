using BrunoMikoski.AnimationSequencer;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.TaggedUsers
{
    public sealed class TaggedUsersPopup: BasePopup<TaggedUsersPopupConfiguration>
    {
        [SerializeField] private TaggedUsersView _view;
        [Space]
        [SerializeField] private AnimationSequencerController _animationSequencer;
        
        protected override void OnConfigure(TaggedUsersPopupConfiguration configuration)
        {
        }

        public override void Show()
        {
            base.Show();
            
            // perform initialization only after popup and child objects are activated
            _view.Initialize(Configs.TaggedGroups);
            
            _view.CloseRequested += Hide;
            
            _animationSequencer.Rewind();
            _animationSequencer.PlayForward();
        }

        public override void Hide(object result)
        {
            _view.CleanUp();
            
            _view.CloseRequested -= Hide;
            
            _animationSequencer.PlayBackwards(true, () => base.Hide(result));
        }
    }
}