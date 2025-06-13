using DG.Tweening;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents
{
    internal class TaskEventHint : TaskHint
    {
        private const float BOUNCE_DISTANCE = 25f;
        

        protected override void PlayAnimation()
        {
            base.PlayAnimation();
            AnimationSequence.Append(RectTransform.DOMoveY(StartPosition.y - BOUNCE_DISTANCE, 1f));
            AnimationSequence.Append(RectTransform.DOMoveY(StartPosition.y, 1f));
            AnimationSequence.SetLoops(3);
            AnimationSequence.OnComplete(FadeOut);
        }
    }
}