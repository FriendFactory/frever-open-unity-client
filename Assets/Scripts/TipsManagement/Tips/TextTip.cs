using System.Collections;
using TipsManagment.Args;
using UnityEngine;
using TMPro;

namespace TipsManagment
{
    public abstract class TextTip : BaseTip
    {
        [SerializeField]
        private TextMeshProUGUI _textUI;
        [SerializeField]
        private RectTransform _arrow;

        public string Text => Args.Text;

        protected CanvasGroup _canvasGroup;

        public override void Init(TipArgs args)
        {
            base.Init(args);
            _canvasGroup = GetComponent<CanvasGroup>();
            SetupText(Args.Text);
        }

        protected virtual void SetupText(string text)
        {
            _textUI.text = text;
        }

        protected override void PositionateTip()
        {
            StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            _canvasGroup.alpha = 0;
            yield return null;
            _canvasGroup.alpha = 1;
            var scaleFactor = RTransform.root.GetComponent<Canvas>().scaleFactor;
            var targetRect = TargetTransform.rect;
            var targetPos = new Vector2(TargetTransform.position.x + targetRect.center.x, TargetTransform.position.y + targetRect.center.y);
            var targetSize = targetRect.size / 2;

            var additionalX = (targetSize.x + Args.Offset.x + RTransform.sizeDelta.x / 2)* scaleFactor;
            var additionalY = (targetSize.y + Args.Offset.y + RTransform.sizeDelta.y / 2)* scaleFactor;

            var newX = targetPos.x;
            var newY = targetPos.y;
            RelativePosition position;
            if (Args.ForcePosition != RelativePosition.None)
            {
                switch (Args.ForcePosition)
                {
                    case RelativePosition.Top:
                        newY += additionalY;
                        break;
                    case RelativePosition.Bottom:
                        newY -= additionalY;
                        break;
                    case RelativePosition.Right:
                        newX += additionalX;
                        break;
                    case RelativePosition.Left:
                        newX -= additionalX;
                        break;
                }
                position = Args.ForcePosition;
            }
            else if (targetPos.x > additionalX)
            {
                newX -= additionalX;
                position = RelativePosition.Left;
            }
            else if ((targetPos.x + additionalX) < Screen.width)
            {
                newX += additionalX;
                position = RelativePosition.Right;
            }
            else if (targetPos.y > additionalY)
            {
                newY -= additionalY;
                position = RelativePosition.Bottom;
            }
            else
            {
                newY += additionalY;
                position = RelativePosition.Top;
            }
            transform.position = new Vector2(newX, newY);
            if(_arrow != null) SetArrowPosition(position);
        }

        protected void SetArrowPosition(RelativePosition position)
        {
            switch (position)
            {
                case RelativePosition.Left:
                    _arrow.anchoredPosition = new Vector2(RTransform.sizeDelta.x / 2 + _arrow.sizeDelta.y / 2, 0);
                    _arrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                    break;
                case RelativePosition.Right:
                    _arrow.anchoredPosition = new Vector2(-(RTransform.sizeDelta.x / 2 + _arrow.sizeDelta.y / 2), 0);
                    _arrow.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
                    break;
                case RelativePosition.Top:
                    _arrow.anchoredPosition = new Vector2(0, -(RTransform.sizeDelta.y / 2 + _arrow.sizeDelta.y / 2));
                    break;
                case RelativePosition.Bottom:
                    _arrow.anchoredPosition = new Vector2(0, RTransform.sizeDelta.y / 2 + _arrow.sizeDelta.y / 2);
                    _arrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
                    break;
            }
        }
    }
}