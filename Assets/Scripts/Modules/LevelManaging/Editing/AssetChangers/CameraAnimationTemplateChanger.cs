using System;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    [UsedImplicitly]
    internal sealed class CameraAnimationTemplateChanger : BaseChanger
    {
        private readonly ICameraTemplatesManager _templatesManager;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CameraAnimationTemplateChanger(ICameraTemplatesManager cameraTemplatesManager)
        {
            _templatesManager = cameraTemplatesManager;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Run(CameraAnimationTemplate target, Action<IAsset> onCompleted)
        {
            InvokeAssetStartedUpdating(DbModelType.CameraAnimationTemplate, target.Id);
            var templateClip = _templatesManager.GetTemplateClipById(target.Id);
            _templatesManager.UpdateStartPositionForTemplate(templateClip);
            _templatesManager.ChangeTemplateAnimation(target.Id);
            onCompleted?.Invoke(null);
            InvokeAssetUpdated(DbModelType.CameraAnimationTemplate);
        }

    }
}