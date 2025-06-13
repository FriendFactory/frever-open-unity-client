using UnityEngine;

namespace Extensions
{
    public static class Texture2DExtensions
    {
        public static Sprite ToSprite(this Texture2D texture2D)
        {
            var rect = new Rect(0f, 0f, texture2D.width, texture2D.height);
            var pivot = Vector2.one * 0.5f;
            return Sprite.Create(texture2D, rect, pivot);
        }
        
        public static void Scale(this Texture2D inputTexture, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            var textureRect = new Rect(0,0,width,height);
            GPUScale(inputTexture,width,height,mode);
            
            inputTexture.Reinitialize(width, height);
            inputTexture.ReadPixels(textureRect,0,0,true);
            inputTexture.Apply(false);  
        }

        private static void GPUScale(Texture2D src, int width, int height, FilterMode filterMode)
        {
            src.filterMode = filterMode;
            src.Apply(false);
            var renderTexture = new RenderTexture(width, height, 32);
            Graphics.SetRenderTarget(renderTexture);
            GL.LoadPixelMatrix(0,1,1,0);
            GL.Clear(true,true,new Color(0,0,0,0));
            Graphics.DrawTexture(new Rect(0,0,1,1),src);
            Object.Destroy(renderTexture);
        }

        public static Texture2D ScaleAndCrop(this Texture2D src, Vector2Int resolution)
        {
            var resultTexture = new Texture2D(src.width, src.height, src.format, false);
            resultTexture.SetPixels(src.GetPixels());
            
            Color[] pixels;
            
            if (src.width > src.height)
            {
                resultTexture.Scale((int)(src.width * ((float)resolution.y / src.height)), resolution.y);
                pixels = resultTexture.GetPixels((resultTexture.width - resolution.x) / 2, 0, resolution.x, resolution.y);
            }
            else
            {
                resultTexture.Scale(resolution.x, (int)(src.height * ((float)resolution.x / src.width)));
                pixels = resultTexture.GetPixels( 0, (resultTexture.height - resolution.y) / 2, resolution.x, resolution.y);
            }
            
            resultTexture.Reinitialize(resolution.x, resolution.y);
            resultTexture.SetPixels(pixels);

            return resultTexture;
        }

        /// <summary>
        /// Converts a non-readable texture to a readable Texture2D.
        /// "targetTexture" can be null or you can pass in an existing texture.
        /// Remember to Destroy() the returned texture after finished with it
        /// </summary>
        public static Texture2D GetReadableTexture(this Texture2D inputTexture, Texture2D targetTexture = null)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                inputTexture.width, inputTexture.height, 0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            if (targetTexture == null)
            {
                targetTexture = new Texture2D(renderTexture.width, renderTexture.height);
            }

            Graphics.Blit(inputTexture, renderTexture);

            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = renderTexture;

            targetTexture.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0, false);
            targetTexture.Apply(false, false);

            RenderTexture.active = prevRT;
            RenderTexture.ReleaseTemporary(renderTexture);

            return targetTexture;
        }
    }
}