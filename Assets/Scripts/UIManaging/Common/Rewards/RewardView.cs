using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Gamification.Reward;
using Bridge.Results;
using Extensions;
using Modules.Sound;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.Rewards
{
    public abstract class RewardView : MonoBehaviour, IRewardView
    {
        private const float DEFAULT_ALPHA = 1f;
        private const float CLAIMED_ALPHA = 0.5f;
        
        [Header("Background")]
        [SerializeField] protected Image _thumbnailBackground;
        [SerializeField] protected Sprite _defaultBackground;
        [SerializeField] private Sprite _claimedBackground;
        
        [Header("Content")]
        [SerializeField] private Animator _glitterAnimator;
        [FormerlySerializedAs("_claimButton")]
        [SerializeField] protected Button _button;
        [SerializeField] protected Image _thumbnail;

        [Space]
        [SerializeField] private bool _hasLockIcon;
        [ShowIf("_hasLockIcon")]
        [SerializeField] private GameObject _lockIcon;
        
        [Header("Claiming")]
        [SerializeField] private Image _claimButtonImage;
        [SerializeField] private GameObject _claimedIcon;
        
        [SerializeField] private ButtonSoundTrigger _soundTrigger;
        
        [Inject] protected IBridge Bridge;
        [Inject] protected RewardEventModel PageModel;

        private static readonly int START = Animator.StringToHash("Start");
        private static readonly int STOP = Animator.StringToHash("Stop");
        
        protected IRewardModel Reward;
        protected RewardState State;

        protected CancellationTokenSource CancellationSource;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            if (State == RewardState.Available)
            {
                _glitterAnimator.SetTrigger(START);
            }
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        //---------------------------------------------------------------------
        // IRewardView
        //---------------------------------------------------------------------

        public virtual void Show(IRewardModel reward, RewardState state)
        {
            ProjectContext.Instance.Container.InjectGameObject(gameObject);
            
            Reward = reward;
            State = state;

            gameObject.SetActive(true);

            switch (state)
            {
                case RewardState.Available:
                    ShowAsAvailable();
                    break;
                case RewardState.Claimed:
                    ShowAsClaimed();
                    break;
                case RewardState.Obtainable:
                    ShowAsObtainable();
                    break;
                case RewardState.Locked:
                    ShowAsLocked();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            
            SetUpSoundTrigger(state);
        }

        public virtual void Hide()
        {
            _button.onClick.RemoveAllListeners();
            
            if (_hasLockIcon) _lockIcon.SetActive(false);
            gameObject.SetActive(false);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected virtual void ShowAsAvailable()
        {
            if (_hasLockIcon) _lockIcon.SetActive(false);
            UpdateAlpha(DEFAULT_ALPHA);
            
            if (_claimButtonImage) _claimButtonImage.SetActive(true);
            _claimedIcon.SetActive(false);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClaimButtonClicked);
            
            _glitterAnimator.SetTrigger(START);
        }
        
        protected virtual void ShowAsClaimed(bool updateThumbnail = true)
        {
            _button.onClick.RemoveAllListeners();

            if (_hasLockIcon) _lockIcon.SetActive(false);
            UpdateAlpha(CLAIMED_ALPHA);
            
            _claimedIcon.SetActive(true);
            if (_claimButtonImage) _claimButtonImage.SetActive(false);
            
            _thumbnailBackground.sprite = _claimedBackground;

            _glitterAnimator.SetTrigger(STOP);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnPreviewRequested);
        }
        
        protected virtual void ShowAsObtainable()
        {
            if (_hasLockIcon) _lockIcon.SetActive(false);
            UpdateAlpha(DEFAULT_ALPHA);
            _thumbnailBackground.sprite = _defaultBackground;
            if (_claimButtonImage) _claimButtonImage.SetActive(false);
            _claimedIcon.SetActive(false);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnPreviewRequested);
        }
        
        protected virtual void ShowAsLocked()
        {
            _thumbnailBackground.sprite = _defaultBackground;
            
            if (_hasLockIcon) _lockIcon.SetActive(true);
            UpdateAlpha(DEFAULT_ALPHA);
            
            if (_claimButtonImage) _claimButtonImage.SetActive(false);
            _claimedIcon.SetActive(false);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnLockedButtonClicked);
        }

        protected abstract void OnClaimButtonClicked();

        protected void OnClaimResult(bool isSuccess)
        {
            if (isSuccess)
            {
                OnClaimSuccess();
                return;
            }

            OnClaimFailed();
        }

        protected virtual void OnClaimSuccess()
        {
            ShowAsClaimed(updateThumbnail: false);
            State = RewardState.Claimed;
        }

        protected virtual void OnClaimFailed()
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClaimButtonClicked);  
        }
        
        private void OnLockedButtonClicked()
        {
            PageModel.OnLockedRewardClicked(Reward.Id);
        }

        private void OnPreviewRequested()
        {
            PageModel.OnPreviewRequested(Reward.Id);
        }
        
        private void SetUpSoundTrigger(RewardState rewardState)
        {
            _soundTrigger.enabled = rewardState == RewardState.Available || rewardState == RewardState.Locked;

            if (rewardState == RewardState.Locked)
            {
                _soundTrigger.ChangeSoundType(ButtonSoundTrigger.ButtonSoundType.Primary);
                return;
            }
            
            _soundTrigger.ChangeSoundType(ButtonSoundTrigger.ButtonSoundType.Claim);
        }
        
        protected async void DownloadThumbnail()
        {
            _thumbnail.SetActive(false);
            CancelThumbnailLoading();
            CancellationSource = new CancellationTokenSource();

            try
            {
                var result = await GetThumbnailRequest();
                if (result != null && result.IsSuccess)
                {
                    var texture = result.Object as Texture2D;
                    if (result.Object is Texture2D)
                    {
                        OnThumbnailLoaded(texture);
                    }
                    else
                    {
                        Debug.LogWarning("Wrong thumbnail format");
                    }
                }
                else
                {
                    Debug.LogWarning(result?.ErrorMessage);
                }

            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            finally
            {
                CancellationSource = null;
            }
        }

        private void CancelThumbnailLoading()
        {
            if (CancellationSource == null) return;
            CancellationSource.Cancel();
            CancellationSource = null;
        }
        
        protected virtual void OnThumbnailLoaded(Texture2D texture)
        {
            if (_thumbnail.IsDestroyed()) return;

            var rect = new Rect(0f, 0f, texture.width, texture.height);
            var pivot = new Vector2(0.5f, 0.5f);
            _thumbnail.sprite = Sprite.Create(texture, rect, pivot);
            _thumbnail.SetActive(true);
        }

        protected virtual void UpdateAlpha(float alpha)
        {
            _thumbnail.color = _thumbnail.color.SetAlpha(alpha);
        }
        
        protected abstract Task<GetAssetResult> GetThumbnailRequest();
    }
}