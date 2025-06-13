using System;
using System.Collections.Generic;
using System.Linq;
using Coffee.UISoftMask;
using Extensions;
using Modules.PhotoBooth.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    internal class ProfilePhotoMaskOverlay : MonoBehaviour
    {
        [Serializable]
        public class MaskSettingsEditorModel
        {
            public ProfilePhotoType photoType;
            public Image background;
        }

        [SerializeField] private Color _color = Color.black;
        [SerializeField] private List<MaskSettingsEditorModel> _maskSettings;
        [Header("UI Components")] 
        [SerializeField] private CanvasGroup _group;
        
        private readonly Dictionary<ProfilePhotoType, Image> _maskImagesMap = new Dictionary<ProfilePhotoType, Image>();

        private List<SoftMask> _masks;

        private void Awake()
        {
            _masks = _group.GetComponentsInChildren<SoftMask>(true).ToList();
            
            _maskSettings.ForEach(settings => _maskImagesMap[settings.photoType] = settings.background);
        }

        public void Initialize(ProfilePhotoType photoType)
        {
            if (!_maskImagesMap.TryGetValue(photoType, out var background))
            {
                return;
            }

            _masks.ForEach(mask => mask.SetActive(false));
            
            background.color = _color;
            background.transform.parent.SetActive(true);
        }

        public void Show()
        {
            _group.SetActive(true);
        }

        public void Hide()
        {
            _group.SetActive(false);
        }
    }
}