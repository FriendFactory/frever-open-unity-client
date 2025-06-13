using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common.Files;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using UnityEngine;

namespace Modules.WatermarkManagement
{
    public interface IWatermarkService
    {
        float Opacity { get; set; }
        bool IsLandscape { get; set; }
        void SetRenderState(bool render);
        void SetTargetCamera(Camera camera);
        void SetIntellectualProperty(IntellectualProperty intellectualProperty);
        Task FetchWaterMarks();
        Task FetchWaterMark(IntellectualProperty intellectualProperty);
        void ReleaseWatermarkTexture();
    }
    
    [UsedImplicitly]
    internal sealed class WatermarkService: IWatermarkService
    {
        private readonly IBridge _bridge;
        private readonly IMetadataProvider _metadataProvider;
        
        public float Opacity
        {
            get => WatermarkSettings.Opacity;
            set => WatermarkSettings.Opacity = value;
        }

        public bool IsLandscape
        {
            get => WatermarkSettings.Orientation == VideoOrientation.Landscape;
            set => WatermarkSettings.Orientation = value ? VideoOrientation.Landscape : VideoOrientation.Portrait;
        }

        public WatermarkService(IBridge bridge, IMetadataProvider metadataProvider)
        {
            _bridge = bridge;
            _metadataProvider = metadataProvider;
        }

        public void SetRenderState(bool render)
        {
            WatermarkSettings.IsOn = render;
        }

        public void SetTargetCamera(Camera camera)
        {
            WatermarkSettings.TargetCamera = camera;
        }

        public void SetIntellectualProperty(IntellectualProperty intellectualProperty)
        {
            var watermark = intellectualProperty.Watermark;
            if (watermark == null) return;
            if (!_bridge.HasCached(watermark, watermark.Files.First()))
            {
                Debug.LogError("Watermark is not fetched");
                return;
            }

            var file = watermark.Files.FirstOrDefault(x=> x.Tags != null && x.Tags.Contains(IsLandscape ? FileInfoTags.LANDSCAPE: FileInfoTags.PORTRAIT));
            if (file == null)
            {
                file = watermark.Files.First();
            }

            WatermarkSettings.Watermark = intellectualProperty.Watermark;
            var watermarkPath = _bridge.GetFilePath(watermark, file);
            var bytes = File.ReadAllBytes(watermarkPath);
            var texture = new Texture2D(2, 2);
            if (texture.LoadImage(bytes))
            {
                WatermarkSettings.WatermarkTexture = texture;
            }
        }

        public async Task FetchWaterMarks()
        {
            var ips = _metadataProvider.MetadataStartPack.IntellectualProperty.Where(x => x.Watermark != null);
            foreach (var ip in ips)
            {
                await FetchWaterMark(ip);
            }
        }

        public async Task FetchWaterMark(IntellectualProperty intellectualProperty)
        {
            if (intellectualProperty.Watermark == null) return;
            foreach (var fileInfo in intellectualProperty.Watermark.Files)
            {
                var res = await _bridge.FetchMainAssetAsync(intellectualProperty.Watermark, fileInfo);
                if (res.IsError)
                {
                    Debug.LogError($"Failed to fetch watermark for {intellectualProperty.Name}. {res.ErrorMessage}");
                }
            }
        }

        public void ReleaseWatermarkTexture()
        {
            if (WatermarkSettings.WatermarkTexture != null)
            {
                Object.Destroy(WatermarkSettings.WatermarkTexture);
            }

            WatermarkSettings.WatermarkTexture = null;
        }
    }
}