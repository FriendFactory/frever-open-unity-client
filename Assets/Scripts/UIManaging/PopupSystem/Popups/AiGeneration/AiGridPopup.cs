using System;
using System.Linq;
using System.Threading;
using Bridge;
using Common;
using Extensions;
using TMPro;
using UIManaging.Animated.Behaviours;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.AiGeneration
{
    internal sealed class AiGridPopup: ConfigurableBasePopup<AiGridPopupConfiguration>
    {
        [Space]
        [SerializeField] private Image _logo;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _cancelButton;
        [Space]
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private AiGridPopupTextItem _itemPrefab;
        [SerializeField] private int _cellWidth = 180;
        [SerializeField] private int _cellHeight = 180;
        [SerializeField] private int _cellHeightWithLable = 245;
        [Space]
        [SerializeField] private SlideInOutBehaviour _slideBehaviour;
        [SerializeField] private FadeInOutBehaviour _fadeBehaviour;

        [Inject] private IBridge _bridge;

        private int _steps;
        private string[] _selectedOptions;

        private CancellationTokenSource _tokenSource;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            if (Config == null) return;

            _cancelButton.onClick.AddListener(OnCancel);
            ShowOptionSet(0);
            InitAndRunFadeAnimation();
        }

        private void OnDisable()
        {
            _cancelButton.onClick.RemoveListener(OnCancel);
            CancelCurrentRequest();
            DestroyCurrentItems();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async void OnConfigure(AiGridPopupConfiguration configuration)
        {
            _steps = configuration.OptionSets.Length;
            _selectedOptions = new string[_steps];

            if (configuration.Logo != null)
            {
                _logo.sprite = configuration.Logo;
            }
            else
            {
                _logo.SetActive(false);

                var token = CreateNewTokenSource().Token;
                var result = await _bridge.GetAssetAsync(configuration.Entity, cancellationToken: token);

                if (result.IsError)
                {
                    Debug.LogWarning("Failed to load AI background texture: " + result.ErrorMessage);
                    return;
                }

                var tex = (result.IsSuccess) ? result.Object as Texture2D : null;
                var sprite = tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)) : null;

                _logo.sprite = sprite;
                _logo.SetAlpha(0);
                _logo.SetActive(true);
                _fadeBehaviour.FadeIn();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowOptionSet(int index)
        {
            DestroyCurrentItems();

            var optionSet = Config.OptionSets[index];

            _titleText.text = optionSet.Title;
            _gridLayout.constraintCount = optionSet.ColumnsCount;

            var showLabels = optionSet.Options.Any(option => !string.IsNullOrEmpty(option.Label));

            _gridLayout.cellSize = (showLabels)
                ? new Vector2(_cellWidth, _cellHeightWithLable)
                : new Vector2(_cellWidth, _cellHeight);

            foreach (var option in optionSet.Options)
            {
                var item = Instantiate(_itemPrefab, _gridLayout.transform);
                var emoji = option.DisplayValue;
                var label = (showLabels) ? option.Label : null;

                item.SetValue(emoji, label);
                item.Button.onClick.AddListener(OnClick);

                void OnClick()
                {
                    _selectedOptions[index] = option.PromptValue;

                    if (_steps > index + 1)
                    {
                        _slideBehaviour.SlideOut(() =>
                        {
                            ShowOptionSet(index + 1);
                        });
                    }
                    else
                    {
                        Config.ConfirmAction?.Invoke(_selectedOptions);
                        SlideAndHide();
                    }
                }
            }

            InitAndRunSlideAnimation();
        }

        private void InitAndRunFadeAnimation()
        {
            foreach (var model in _fadeBehaviour.Models)
            {
                switch (model.Target)
                {
                    case FadeInOutModel.TargetType.Graphic:
                        model.Graphic.SetAlpha(model.AlphaTargetOut);
                        break;
                    case FadeInOutModel.TargetType.CanvasGroup:
                        model.CanvasGroup.alpha = model.AlphaTargetOut;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _fadeBehaviour.FadeIn();
        }

        private void InitAndRunSlideAnimation()
        {
            CoroutineSource.Instance.ExecuteAtEndOfFrame(() =>
            {
                var inPosition = ((RectTransform) _slideBehaviour.transform).rect.height * Vector3.up;
                var outPosition = Vector3.zero;

                _slideBehaviour.InitSequence(inPosition, outPosition);
                _slideBehaviour.Hide();
                _slideBehaviour.SlideIn();
            });
        }

        private void OnCancel()
        {
            SlideAndHide();
        }

        private void SlideAndHide()
        {
            _fadeBehaviour.FadeOut();
            _slideBehaviour.SlideOut(Hide);
        }

        private void DestroyCurrentItems()
        {
            foreach (Transform child in _gridLayout.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private CancellationTokenSource CreateNewTokenSource()
        {
            _tokenSource?.CancelAndDispose();
            _tokenSource = new CancellationTokenSource();
            return _tokenSource;
        }

        private void CancelCurrentRequest()
        {
            _tokenSource?.CancelAndDispose();
            _tokenSource = null;
        }
    }
}