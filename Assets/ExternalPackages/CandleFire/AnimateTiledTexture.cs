using System.Collections;
using UnityEngine;

namespace ExternalPackages.CandleFire
{
    public sealed class AnimateTiledTexture: MonoBehaviour
    {
        public int columns = 2;
        public int rows = 2;
        public float framesPerSecond = 10f;
    
        private int _index = 0;
        private Renderer _renderer;

        private void Start()
        {
            StartCoroutine(updateTiling());
        
            var size = new Vector2(1f / columns, 1f / rows);
            _renderer = GetComponent<Renderer>();
            _renderer.sharedMaterial.SetTextureScale("_MainTex", size);
        }
	
        private IEnumerator updateTiling()
        {
            while (true)
            {
                //move to the next index
                _index++;
                if (_index >= rows * columns)
                    _index = 0;
			
                //split into x and y indexes
                var offset = new Vector2((float)_index / columns - (_index / columns), //x index
                                         (_index / columns) / (float)rows);           //y index
			
                _renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
			
                yield return new WaitForSeconds(1f / framesPerSecond);
            }
		
        }
    }
}