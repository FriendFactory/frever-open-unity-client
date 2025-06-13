using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common.Files;
using Extensions;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;
using CameraAnimationTemplate = Bridge.Models.ClientServer.StartPack.Metadata.CameraAnimationTemplate;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public class CameraAssetSelector : MainAssetSelectorModel
    {
        private readonly MetadataStartPack _metadata;
        private readonly CameraAnimationTemplate[] _templates;
        
        public override DbModelType AssetType => DbModelType.CameraAnimation;

        public CameraAssetSelector(string displayName, ICategory[] categories, MetadataStartPack metadata, CameraAnimationTemplate[] templates) : 
            base(displayName, categories)
        {
            _metadata = metadata;
            _templates = templates;
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(1, true);
            SetAssetSelectionHandler(assetSelectionHandler);
        }

        public override Task DownloadInitialPage(long categoryId, bool force = false, CancellationToken token = default)
        {
            var cameraAnimationTemplates = _metadata.CameraCategories
                                                       .Where(category => category.CameraAnimationTemplates != null)
                                                       .SelectMany(category => category.CameraAnimationTemplates.Select(template => (category, template)))
                                                       .Distinct()
                                                       .Where(x=> _templates.Contains(x.template)) 
                                                       .OrderBy(tuple => tuple.template.SortOrder)
                                                       .ToArray();
            var index = 0;
            var selectableItems = cameraAnimationTemplates.Select(tuple =>
                                                                      new AssetSelectionCameraModel(index++, 
                                                                          Resolution._128x128, 
                                                                          tuple.template, 
                                                                          tuple.category.Name, 
                                                                          tuple.category.Id));

            AddItems(selectableItems, true);
            
            return Task.CompletedTask;
        }

        public override void SetSelectedItemsAsInEvent(ILevelManager levelManager, Event @event,
            IDataFetcher dataFetcher = null,
            bool silent = true)
        {
            base.SetSelectedItemsAsInEvent(levelManager, @event, dataFetcher, silent);
        
            var cameraAnimationTemplates = AllItems.Select(itm => itm.RepresentedObject).Cast<CameraAnimationTemplate>().ToArray();
            var cameraAnimation = @event?.GetCameraController();
            CameraAnimationTemplate selectedTemplateAnimation;

            if (cameraAnimation == null)
            {
                selectedTemplateAnimation = cameraAnimationTemplates.FirstOrDefault();
            }
            else if (cameraAnimationTemplates.Any(x=>x.Id == cameraAnimation.CameraAnimationTemplateId))
            {
                selectedTemplateAnimation = cameraAnimationTemplates.FirstOrDefault(cam => cam.Id.Equals(cameraAnimation.CameraAnimationTemplateId));
            }
            else
            {
                var (cameraCategory, cameraAnimationTemplate) = _metadata.CameraCategories
                   .Where(category => category.CameraAnimationTemplates != null)
                   .SelectMany(
                        category => category.CameraAnimationTemplates.Select(
                            template => (category, template)))
                   .FirstOrDefault(tuple => tuple.template.Id ==
                                            cameraAnimation.CameraAnimationTemplateId);
                selectedTemplateAnimation = cameraAnimationTemplate;
                
                AddItems(new [] { new AssetSelectionCameraModel(Resolution._128x128, 
                                                       cameraAnimationTemplate, 
                                                       cameraCategory.Name, 
                                                       cameraCategory.Id) });
            }

            if (!silent)
            {
                AssetSelectionHandler.UnselectAllSelectedItems();
            }

            if (selectedTemplateAnimation == null)
            {
                return;
            }

            SetSelectedItems(new[] { selectedTemplateAnimation.Id }, silent:silent);
        }

        public override bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            return AssetSelectionHandler.SelectedModels.Count > 0 && AssetSelectionHandler.SelectedModels[0].ItemId == @event.GetCameraController().CameraAnimationTemplateId;
        }
    }
}