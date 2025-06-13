using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.VideoStreaming.Remix.Selection
{
    internal class SelectedCharacterButton : MonoBehaviour
    {
        [SerializeField] private Sprite _standardImage;
        [SerializeField] private Image _image;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _selectedGameObject;
        [SerializeField] private TMP_Text _indexText;

        private long _originalId;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public long NewId { get; private set; } = -1;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<long> Clicked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Start()
        {
            _button.onClick.AddListener(OnClicked);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(Sprite image, long id, bool canBeRemoved = true)
        {
            NewId = id;
            SetImage(image);
            _button.interactable = canBeRemoved;
        }
        
        public void ResetValues()
        {
            NewId = -1;
            _image.sprite = _standardImage;
            SetImage(null);
        }

        public void SetIndex(int index)
        {
            _indexText.text = index.ToString();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetImage(Sprite image)
        {
            var imageNull = image == null;
            
            _image.sprite = imageNull ? _standardImage : image;
            _image.enabled = !imageNull;
            _selectedGameObject.SetActive(imageNull == false);
        }

        private void OnClicked()
        {
            Clicked?.Invoke(NewId);
            ResetValues();
        }
    }
}
