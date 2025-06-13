using DG.Tweening;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents
{
    internal class TaskAssetHint : TaskHint
    {
        private const float BOUNCE_DISTANCE = 25f;
        
        [SerializeField] private DbModelType _type;

        public DbModelType Type => _type;

        protected override void PlayAnimation()
        {
            base.PlayAnimation();
            AnimationSequence.Append(RectTransform.DOMoveX(StartPosition.x + BOUNCE_DISTANCE, 1f));
            AnimationSequence.Append(RectTransform.DOMoveX(StartPosition.x, 1f));
            AnimationSequence.SetLoops(3);
            AnimationSequence.OnComplete(FadeOut);
        }
    }
}
