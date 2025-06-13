using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using TMPro;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Color = UnityEngine.Color;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Feed.Remix.Collection
{
    public class CharacterButton : BaseContextDataView<CharacterButtonModel>
    {
        [SerializeField] private Button button;
        [SerializeField] private Image thumbnail;
        [SerializeField] private TextMeshProUGUI characterName;
        [SerializeField] private GameObject selectedImage;
        [SerializeField] private TextMeshProUGUI borderCount;
        [SerializeField] private CanvasGroup canvasGroup;

        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;

        private CancellationTokenSource _cancellationSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public long Id => Character.Id;
        public CharacterInfo Character { get; private set;}

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Start()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _cancellationSource?.Cancel();
            
            CancelAllRequests();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public async Task FetchThumbnailAsync(CancellationToken token = default)
        {
            thumbnail.enabled = false;
            
            if (_bridge == null) return;
            
            var thumbnailFile = Character.Files.First(x => x.Resolution == Resolution._128x128);
            var result = await _bridge.GetCharacterThumbnailAsync(Character.Id, thumbnailFile, true, token);

            if (token.IsCancellationRequested) return;
            if (result.IsSuccess)
            {
                LoadSprite((Texture2D) result.Object);
            }
            else
            {
                Debug.LogWarning($"Could not find thumbnail image for character id: {Character.Id}");
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override async void OnInitialized()
        {
            CancelAllRequests();
            _cancellationSource = new CancellationTokenSource();
            
            Character = ContextData.Character;
            characterName.text = ContextData.NickName;

            if (!ContextData.Thumbnail)
            {
                await FetchThumbnailAsync(_cancellationSource.Token);
            }

            if (_cancellationSource.IsCancellationRequested)
            {
                return;
            }
            
            UpdateThumbnail(ContextData.Thumbnail);
            SetBorderCount(ContextData.BorderCount);

            ContextData.BorderCountChanged -= SetBorderCount;
            ContextData.BorderCountChanged += SetBorderCount;
        }

        protected override void BeforeCleanup()
        {
            SetBorderCount(-1);
            
            ContextData.BorderCountChanged -= SetBorderCount;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetBorderCount(int count)
        {
            borderCount.text = count.ToString();
            selectedImage.SetActive(count >= 0);
        }
       
        private void OnClick()
        {
            if (ContextData.CheckAccess && Character.Access == CharacterAccess.Private)
            {
                _snackBarHelper.ShowInformationSnackBar($"User {Character.Name} doesn't allow anyone to use their character");
            }
            else
            {
                ContextData.OnClick?.Invoke(ContextData);
            }
        }

        private void LoadSprite(Texture2D texture)
        {
            if (texture == null) return;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            
            ContextData.Thumbnail = sprite;
        }

        private void UpdateThumbnail(Sprite sprite)
        {
            thumbnail.sprite = sprite;
            thumbnail.color = Color.white;
            thumbnail.enabled = true;
            canvasGroup.alpha = ContextData.CheckAccess && Character.Access == CharacterAccess.Private ? 0.5f : 1;
        }

        private void CancelAllRequests()
        {
            _cancellationSource?.Cancel();
            _cancellationSource = null;
        }
    }
}
