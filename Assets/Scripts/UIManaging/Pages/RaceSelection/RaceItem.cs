using System;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using Modules.AssetsStoraging.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.RaceSelection
{
    [RequireComponent(typeof(Button))]
    internal sealed class RaceItem: MonoBehaviour
    {
        private const Resolution RESOLUTION = Resolution._512x512;
        private readonly Color _disabledColor = new (.226f, .226f, .226f, 1f);
        private readonly Color _enabledColor = Color.white;
        
        [SerializeField] private TMP_Text _text;
        [SerializeField] private RawImage _image;
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private GameObject _newItemTag;

        private CancellationTokenSource _cancellationSource;
        
        private Race _race;

        public event Action<Race> Clicked; 

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
            _loadingSpinner.SetActive(false);
            _image.color = _disabledColor;
        }

        private void OnDestroy()
        {
            if (_cancellationSource is null)
            {
                return;
            }
            _cancellationSource.Cancel();
            _cancellationSource = null;
            
            
            if (_image.texture)
            {
                Destroy(_image.texture);
            }
        }

        public async void Init(Race race, IMetadataProvider metadataProvider, IBridge bridge)
        {
            _race = race;

            _text.enabled = true;
            _image.color = _disabledColor;
            
            var universe = metadataProvider.MetadataStartPack.GetUniverseByRaceId(race.Id);
            
            _newItemTag.SetActive(universe.IsNew);

            var thumbnailFileInfo = universe.Files?.FirstOrDefault(f => f.Resolution == RESOLUTION);
            if (thumbnailFileInfo is null)
            {
                Debug.LogError($"The thumbnail for the Universe '{universe.Name}' (ID={universe.Id} wasn't found");
                return;
            }
            _text.enabled = false;
            
            if (bridge.HasCached(universe, thumbnailFileInfo))
            {
                _image.color = _enabledColor;
                _image.texture = bridge.GetThumbnailFromCacheImmediate(universe, RESOLUTION).Model;
            }
            else
            {
                _loadingSpinner.SetActive(true);
                
                _cancellationSource = new CancellationTokenSource();

                var result = await bridge.GetThumbnailAsync(universe, RESOLUTION, true, _cancellationSource.Token);
                if (this.IsDestroyed() || (_cancellationSource is not null && _cancellationSource.IsCancellationRequested))
                {
                    return;
                }
                _image.texture = result.Object as Texture2D;
                
                _loadingSpinner.SetActive(false);
                _image.color = _enabledColor;
                _cancellationSource = null;
            }
        }

        private void OnClicked()
        {
            Clicked?.Invoke(_race);
        }
    }
}