using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    public sealed class CharacterSwitchGroupButton : CharacterSwitchButton
    {
        [SerializeField] private Sprite _duoGroupSprite;
        [SerializeField] private Sprite _trioGroupSprite;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void RefreshThumbnail()
        {
            var charactersCount = LevelManager.TargetEvent.CharacterController.Count;

            switch (charactersCount)
            {
                case 2:
                    _thumbnail.sprite = _duoGroupSprite;
                    break;
                case 3:
                    _thumbnail.sprite = _trioGroupSprite;
                    break;
                default:
                    _thumbnail.sprite = null;
                    break;
            };

        }
    }
}