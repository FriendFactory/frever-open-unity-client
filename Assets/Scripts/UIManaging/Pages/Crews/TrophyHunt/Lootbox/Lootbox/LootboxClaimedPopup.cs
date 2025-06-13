using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.UserActivity;
using DG.Tweening;
using Extensions;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Crews.TrophyHunt.Lootbox;
using UIManaging.Pages.Crews.TrophyHunt.Lootbox.Lootbox;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.PopupSystem.Popups.Lootbox
{
    public class LootboxClaimedPopup : BasePopup<LootboxClaimedPopupConfiguration>
    {
        [SerializeField] private LootboxRewardsScroller _rewardsScroller;
        [SerializeField] private ParticleSystem _confettiVfx;
        [SerializeField] private Image _lootboxThumbnail;

        [SerializeField] private CanvasGroup _lootboxPreviewGroup;
        [SerializeField] private CanvasGroup _lootboxRouletteGroup;
        [SerializeField] private CanvasGroup _buttonCanvasGroup;
        
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _buttonText;

        [SerializeField] private TMP_Text _lootboxRarityText;

        [Inject] private IBridge _bridge;
        [Inject] private SnackBarManager _snackBarManager;
        [Inject] private LootboxPopupLocalization _localization;
        
        private readonly Dictionary<long, Texture2D> _thumbnails = new Dictionary<long, Texture2D>();

        protected override void OnConfigure(LootboxClaimedPopupConfiguration configuration)
        {
            _lootboxThumbnail.sprite = Configs.Thumbnail;
            _lootboxPreviewGroup.alpha = 1;
            _lootboxRouletteGroup.alpha = 0;
            
            _lootboxPreviewGroup.interactable = true;
            _lootboxPreviewGroup.blocksRaycasts = true;
            
            _lootboxRouletteGroup.interactable = false;
            _lootboxRouletteGroup.blocksRaycasts = false;
            
            _lootboxRarityText.text = string.Format(_localization.OpenLootboxDescriptionFormat, _localization.GetRarityLocalized(configuration.LootboxTitle));
            
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnOpenLootboxButton);
            _buttonText.text = _localization.OpenButton;

            PrefetchThumbnails();
            
            _button.transform.localScale = Vector3.one;
            _buttonCanvasGroup.alpha = 1;
        }

        private async void PrefetchThumbnails()
        {
            var tasks = new List<Task>();

            _thumbnails.Clear();
            
            foreach (var reward in Configs.PossibleRewards)
            {
                var task = FetchThumbnail(reward);
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);

            _rewardsScroller.Initialize(Configs.PossibleRewards.Select(reward => new LootboxRewardModel
            {
                Asset = reward, 
                Thumbnail = _thumbnails[reward.Id]
            }).ToArray());
        }
        
        async Task FetchThumbnail(LootBoxAsset reward)
        {
            var result = await _bridge.GetThumbnailAsync(reward.Asset, Resolution._128x128);
            _thumbnails[reward.Id] = result.Object as Texture2D;
        }
        
        private void OnOpenLootboxButton()
        {
            FadeGroup(_lootboxPreviewGroup, false);
            FadeGroup(_lootboxRouletteGroup, true);
            
            var selectedIndex = Configs.PossibleRewards.ToList()
                                        .FindIndex(reward => reward.Id == Configs.Reward.Id);
            _rewardsScroller.Spin(selectedIndex, OnSpinCompleted);
            
            _button.onClick.RemoveAllListeners();
            PlayCloseButtonAnimation(false);
        }

        private void OnSpinCompleted()
        {
            _confettiVfx.Play();
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(Hide);
            _button.interactable = true;
            PlayCloseButtonAnimation(true);
            _buttonText.text = _localization.ConfirmButton;
        }

        private void FadeGroup(CanvasGroup canvasGroup, bool visible)
        {
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.DOFade(visible ? 1 : 0, 0.3f);
        }

        public override void Hide()
        {
            var texture = _thumbnails[Configs.Reward.Id];
            _thumbnails.Remove(Configs.Reward.Id);
            var sprite = texture.ToSprite();
            var assetConfiguration = new AssetSnackBarConfiguration(_localization.AssetClaimedSnackbarTitle, 
                                                                    _localization.AssetClaimedSnackbarMessage,
                                                                    sprite);
            _snackBarManager.Show(assetConfiguration);
            _thumbnails.Values.ForEach(value => Destroy(value));
            _thumbnails.Clear();
            base.Hide();
        }

        private void PlayCloseButtonAnimation(bool state)
        {
            if (state)
            {
                _button.transform.localScale = new Vector3(0,0,1);
                _button.transform.DOScale(Vector3.one, 0.35f);
                _buttonCanvasGroup.DOFade(1f, 0.2f);
            }
            else
            {
                _buttonCanvasGroup.DOFade(0f, 0.25f);
            }
        }
    }
}

