using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews
{
    public class CrewChatThumbnailItem : MonoBehaviour
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private Button _removeButton;
        [SerializeField] private Texture2D _defaultThumbnail;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public Button RemoveButton => _removeButton;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void UpdateThumbnail(Texture2D thumbnail)
        {
            if (thumbnail == null)
            {
                _image.texture = _defaultThumbnail;
                return;
            }

            _image.texture = thumbnail;
        }

        public void DestroyThumbnailTexture()
        {
            var texture = _image.texture;
            if (texture == _defaultThumbnail || texture == null) return;
            DestroyImmediate(texture, true);
        }
    }
}