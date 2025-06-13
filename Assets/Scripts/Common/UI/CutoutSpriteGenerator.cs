using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public class CutoutSpriteGenerator : MonoBehaviour
    {
        [SerializeField] private Image _targetImage;
        [SerializeField] private Sprite _graphicForErase;
        [SerializeField] private Vector2Int _erasePosition;
        [SerializeField] private byte _alpha = 255;

        private Texture2D _newTexture;
        private Sprite _newSprite;

        private void Awake()
        {
            EraseGraphic();
            
            _targetImage.alphaHitTestMinimumThreshold = 0.5f;
        }

        private void OnDestroy()
        {
            Destroy(_newSprite);
            Destroy(_newTexture);
        }

        private void EraseGraphic()
        {
            if (_newTexture != null)
            {
                Destroy(_newSprite);
                Destroy(_newTexture);
            }
            
            _newTexture = new Texture2D(_graphicForErase.texture.width, _graphicForErase.texture.height);
            var emptyColor = new Color32(0, 0, 0, 0);
            var notEmptyColor = new Color32(0, 0, 0, _alpha);
            var pixels = Enumerable.Repeat(notEmptyColor, _graphicForErase.texture.width * _graphicForErase.texture.height).ToArray();
            _newTexture.SetPixels32(pixels);

            for (int i = 0; i < _graphicForErase.texture.width; i++)
            {
                for (int j = 0; j < _graphicForErase.texture.height; j++)
                {
                    if (_graphicForErase.texture.GetPixel(i, j).a == 0)
                    {
                        _newTexture.SetPixel(_erasePosition.x + i, _erasePosition.y + j, notEmptyColor);
                    }
                    else
                    {
                        _newTexture.SetPixel(_erasePosition.x + i, _erasePosition.y + j, emptyColor);
                    }
                }
            }
            _newTexture.Apply();
            _newSprite = Sprite.Create(_newTexture, new Rect(0, 0, _graphicForErase.texture.width, _graphicForErase.texture.height), 
                                       _graphicForErase.pivot, _graphicForErase.pixelsPerUnit, 0, SpriteMeshType.FullRect, _graphicForErase.border);
            _targetImage.sprite = _newSprite;
        }
    }
}
