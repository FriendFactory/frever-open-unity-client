using System;
using Navigation.Args;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Modules.GalleryVideoManaging
{
    [RequireComponent(typeof(Button))]
    public sealed class UploadGalleryButton : MonoBehaviour
    {
        [Inject] private IUploadGalleryVideoService _uploadGalleryVideoService;

        public event Action<NonLeveVideoData> VideoSelected; 
        
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _uploadGalleryVideoService.TryToOpenVideoGallery(x=> VideoSelected?.Invoke(x));
        }
    }
}
