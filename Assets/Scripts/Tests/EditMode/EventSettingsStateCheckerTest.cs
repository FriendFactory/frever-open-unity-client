using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Models;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Moq;
using NUnit.Framework;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker;
using AnimationCurve = UnityEngine.AnimationCurve;

namespace Tests.EditMode
{
    internal sealed class EventSettingsStateCheckerTest
    {
        private readonly IAssetStateComparer[] _baseAssetStateComparers =
        {
            new BodyAnimationStateComparer(),
            CreateCameraAnimationTemplateStateComparer(),
            new CameraFilterStateComparer(),
            new CameraFilterVariantStateComparer(),
            new CaptionStateComparer(),
            new CharacterSpawnFormationStateComparer(),
            new CharacterSpawnPositionStateComparer(),
            new CharacterStateComparer(),
            new OutfitStateComparer(),
            new SetLocationStateComparer(),
            new VfxStateComparer(),
        };

        private static CameraAnimationTemplateStateComparer CreateCameraAnimationTemplateStateComparer()
        {
            var levelManagerMock = new Mock<ILevelManager>();
            var cameraAnimationMock = new Mock<ICameraAnimationAsset>();
            var mockCameraAnimationCurves = new Dictionary<CameraAnimationProperty, AnimationCurve>();
            foreach (CameraAnimationProperty cameraAnimProperty in Enum.GetValues(typeof(CameraAnimationProperty)))
            {
                var animCurve = new AnimationCurve();
                animCurve.AddKey(0, 0);
                animCurve.AddKey(10, 100);
                mockCameraAnimationCurves[cameraAnimProperty] = animCurve;
            }

            var cameraAnimationClip = new RecordedCameraAnimationClip(mockCameraAnimationCurves);
            cameraAnimationMock.Setup(x => x.Clip)
                               .Returns(() => cameraAnimationClip);
            levelManagerMock.Setup(x => x.GetCurrentCameraAnimationAsset()).Returns(() => cameraAnimationMock.Object);

            var cameraTemplatesManagerMock = new Mock<ICameraTemplatesManager>();
            cameraTemplatesManagerMock.Setup(x => x.TemplatesStartFrame).Returns(() => cameraAnimationClip.GetFrame(0));
            return new CameraAnimationTemplateStateComparer(levelManagerMock.Object, cameraTemplatesManagerMock.Object);
        }

        private readonly EventSettingsStateChecker _eventSettingsStateChecker;

        public EventSettingsStateCheckerTest()
        {
            _eventSettingsStateChecker = GetSettingsStateChecker();
        }

        [Test]
        public void CheckIfCameraAnimationTemplateIdWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.CameraController.First().CameraAnimationTemplateId = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.CameraAnimationTemplate));
        }
        
        [Test]
        public void CheckIfCameraAnimationTemplateSpeedWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.CameraController.First().TemplateSpeed = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.CameraAnimationTemplate));
        }
        
        [Test]
        public void CheckIfFirstCharacterBodyAnimationWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.CharacterController.First().CharacterControllerBodyAnimation.First().BodyAnimationId = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.BodyAnimation));
        }
        
        [Test]
        public void CheckIfCameraFilterWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.CameraFilterController.First().CameraFilter.Id = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.CameraFilter));
        }
        
        [Test]
        public void CheckIfCameraFilterIntensityWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.CameraFilterController.First().CameraFilterValue = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.CameraFilter));
        }
        
        [Test]
        public void CheckIfCameraFilterVariantIdWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.CameraFilterController.First().CameraFilterVariantId = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.CameraFilterVariant));
        }

        [Test]
        public void CheckIfSetLocationIdWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.SetLocationController.First().SetLocationId = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.SetLocation));
        }
        
        [Test]
        public void CheckIfSpawnPositionWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.CharacterSpawnPositionId = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.CharacterSpawnPosition));
        }
        
        [Test]
        public void CheckIfSpawnFormationWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.CharacterSpawnPositionFormationId = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.SpawnFormation));
        }
        
        [Test]
        public void CheckIfVfxWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.VfxController.First().VfxId = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.Vfx));
        }
        
        [Test]
        public void CheckIfOutfitWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            ev.CharacterController.First().OutfitId = -1;
            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.Outfit));
        }
        
        [Test]
        public void CheckIfCharacterWasModified()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            
            foreach (var controller in ev.CharacterController)
            {
                controller.ControllerSequenceNumber--;
            }

            Assert.IsTrue(_eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.Character));
        }
        
        [Test]
        public void TryCheckChangedAssetWithoutEvent()
        {
            var eventSettingsStateChecker = GetSettingsStateChecker();
            Assert.Catch<InvalidOperationException>(() => eventSettingsStateChecker.HasAnyAssetsChanged(DbModelType.Character));
        }
        
        [Test]
        public void TryCheckChangedAssetWithoutSpecifiedAssetTypes()
        {
            var ev = GetTestEvent();
            _eventSettingsStateChecker.StoreSettings(ev);
            Assert.Catch<InvalidOperationException>(() => _eventSettingsStateChecker.HasAnyAssetsChanged());
        }

        private Event GetTestEvent()
        {
            var ev = new Event
            {
                CharacterSpawnPositionFormationId = 1,
                CharacterSpawnPositionId = 1,
                TargetCharacterSequenceNumber = 0,
                CameraFilterController = {new CameraFilterController{CameraFilterValue = 1, CameraFilter = new CameraFilterInfo{Id = 1}}},
                CameraController = {new CameraController { CameraAnimationTemplateId = 1, TemplateSpeed = 1 }},
                VfxController = {new VfxController{VfxId = 1}},
                SetLocationController = {new SetLocationController{SetLocationId = 1}},
            };

            for (var n = 0; n < 2; n++)
            {
                ev.CharacterController.Add(new CharacterController
                {
                    ControllerSequenceNumber = n,
                    CharacterId = n,
                    OutfitId = n,
                    CharacterControllerBodyAnimation = {new CharacterControllerBodyAnimation{BodyAnimationId = n}}
                });
            }

            return ev;
        }

        private EventSettingsStateChecker GetSettingsStateChecker()
        {
            return new EventSettingsStateChecker(_baseAssetStateComparers);
        }
    }
}
