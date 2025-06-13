using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.PreviewSplitting;
using Modules.MemoryManaging;
using NSubstitute;
using NUnit.Framework;
using CharacterController = Models.CharacterController;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Tests.EditMode
{
    internal sealed class LevelPreviewSplitterTests
    {
        [Test]
        [TestCase(650, 100, 50, 200, 1, 50)]
        [TestCase(1100, 150, 200, 100, 2, 100)]
        [TestCase(825, 200, 100, 500, 1, 25)]
        public void SplitLevelPreviewWithAllUniqueAssets_MustReturnSupposedEventsCount(
            int availableRamMb, int memoryPerSetLocationMb, int memoryPerCharacterMb, int memoryForOthersMb, int charactersPerEvent, int memoryForUmaBuildProcess)
        {
            //arrange
            var memoryManager = Substitute.For<IMemoryManager>();
            memoryManager.GetFreeRamSizeMb().Returns(availableRamMb);
            var assetManager = Substitute.For<IAssetManager>();
            assetManager.GetAllLoadedAssets().Returns(Array.Empty<IAsset>());
                        
            var splitter = new PreviewSplitter(memoryManager, assetManager, memoryPerSetLocationMb, memoryPerCharacterMb, memoryForOthersMb, memoryForUmaBuildProcess);
            
            var level = new Level();
            for (var i = 0; i < 10; i++)
            {
                var ev = new Event();
                ev.LevelSequence = i;
                ev.SetLocationController.Add(new SetLocationController()
                {
                    SetLocationId = i
                });

                for (var n = 0; n < charactersPerEvent; n++)
                {
                    ev.CharacterController.Add(new CharacterController()
                    {
                        CharacterId = i*100 + n
                    });
                }
                
                level.Event.Add(ev);
            }

            var sizePerEvent = memoryPerCharacterMb * charactersPerEvent + memoryPerSetLocationMb;

            var expectedEventsCount = (availableRamMb - memoryForOthersMb - memoryForUmaBuildProcess) / sizePerEvent;

            //action
            var piece = splitter.GetNextPiece(level.Event, SplitType.KeepAssetAsMuchAsAllowedByRam);

            //assert
            Assert.IsTrue(piece.Events.Length == expectedEventsCount);
        }
        
        [Test]
        [TestCase(500, 100, 50, 200, 1, 50)]
        [TestCase(550, 150, 200, 100, 2, 100)]
        [TestCase(525, 200, 100, 500, 1, 25)]
        public void SplitLevelPreviewWithAllUniqueAssets_MockLoadedAssets_MustReturnSupposedEventsCount(
            int availableRamMb, int memoryPerSetLocationMb, int memoryPerCharacterMb, int memoryForOthersMb, int charactersPerEvent, int memoryForUmaBuildProcess)
        {
            var memoryManager = Substitute.For<IMemoryManager>();
            memoryManager.GetFreeRamSizeMb().Returns(availableRamMb);
            
            //arrange
            var level = new Level();
            for (var i = 0; i < 10; i++)
            {
                var ev = new Event();
                ev.LevelSequence = i;
                ev.SetLocationController.Add(new SetLocationController()
                {
                    SetLocationId = i
                });

                for (var n = 0; n < charactersPerEvent; n++)
                {
                    ev.CharacterController.Add(new CharacterController()
                    {
                        CharacterId = i*100 + n
                    });
                }
                
                level.Event.Add(ev);
            }

            var preloadedAssets = new List<IAsset>();
            var firstEvent = level.GetFirstEvent();
            foreach (var characterController in firstEvent.CharacterController)
            {
                var characterAsset = Substitute.For<ICharacterAsset>();
                characterAsset.AssetType.Returns(DbModelType.Character);
                characterAsset.Id.Returns(characterController.CharacterId);
                characterAsset.OutfitId.Returns(characterController.OutfitId);
                preloadedAssets.Add(characterAsset);
            }

            var setLocationAsset = Substitute.For<ISetLocationAsset>();
            setLocationAsset.AssetType.Returns(DbModelType.SetLocation);
            setLocationAsset.Id.Returns(firstEvent.GetSetLocationId());
            preloadedAssets.Add(setLocationAsset);

            int alreadyLoadedMb = 0;
            foreach (var asset in preloadedAssets)
            {
                switch (asset.AssetType)
                {
                    case DbModelType.SetLocation:
                        alreadyLoadedMb += memoryPerSetLocationMb;
                        break;
                    case DbModelType.Character:
                        alreadyLoadedMb += memoryPerCharacterMb;
                        break;
                }
            }
            
            var assetManager = Substitute.For<IAssetManager>();
            assetManager.GetAllLoadedAssets().Returns(preloadedAssets.ToArray());
            
            var splitter = new PreviewSplitter(memoryManager, assetManager, memoryPerSetLocationMb, memoryPerCharacterMb, memoryForOthersMb, memoryForUmaBuildProcess);

            var sizePerEvent = memoryPerCharacterMb * charactersPerEvent + memoryPerSetLocationMb;

            var expectedEventsCount = (availableRamMb + alreadyLoadedMb - memoryForOthersMb - memoryForUmaBuildProcess) / sizePerEvent;

            //action
            var piece = splitter.GetNextPiece(level.Event, SplitType.KeepAssetAsMuchAsAllowedByRam);

            //assert
            Assert.IsTrue(piece.Events.Length == expectedEventsCount);
        }
        
        [Test]
        [TestCase(650, 100, 50, 200, 1, 50)]
        [TestCase(1050, 150, 200, 100, 2, 50)]
        [TestCase(850, 200, 100, 500, 1, 50)]
        public void SplitLevelPreviewWithAllUniqueAssets_MustReturnSingleEvent(
            int availableRamMb, int memoryPerSetLocationMb, int memoryPerCharacterMb, int memoryForOthersMb, int charactersPerEvent, int memoryForUmaBuildProcess)
        {
            //arrange
            var memoryManager = Substitute.For<IMemoryManager>();
            memoryManager.GetFreeRamSizeMb().Returns(availableRamMb);
            var assetManager = Substitute.For<IAssetManager>();
            assetManager.GetAllLoadedAssets().Returns(Array.Empty<IAsset>());
            
            var splitter = new PreviewSplitter(memoryManager, assetManager, memoryPerSetLocationMb, memoryPerCharacterMb, memoryForOthersMb, memoryForUmaBuildProcess);
            
            var level = new Level();
            for (var i = 0; i < 10; i++)
            {
                var ev = new Event();
                ev.LevelSequence = i;
                ev.SetLocationController.Add(new SetLocationController()
                {
                    SetLocationId = i
                });

                for (var n = 0; n < charactersPerEvent; n++)
                {
                    ev.CharacterController.Add(new CharacterController()
                    {
                        CharacterId = i*100 + n
                    });
                }
                
                level.Event.Add(ev);
            }

            //action
            var piece = splitter.GetNextPiece(level.Event, SplitType.KeepAssetsInRamFromOneEventMax);

            //assert
            Assert.IsTrue(piece.Events.Length == 1);
        }
        
        [Test]
        [TestCase(700, 100, 50, 200, 1, 100)]
        [TestCase(1050, 150, 200, 100, 2, 50)]
        [TestCase(900, 200, 100, 500, 1, 100)]
        public void SplitLevelPreviewWithTheSameAssets_MustReturnAllEvents(
            int availableRamMb, int memoryPerSetLocationMb, int memoryPerCharacterMb, int memoryForOthersMb, int charactersPerEvent, int memoryForUmaBuildProcess)
        {
            //arrange
            var memoryManager = Substitute.For<IMemoryManager>();
            memoryManager.GetFreeRamSizeMb().Returns(availableRamMb);
            var assetManager = Substitute.For<IAssetManager>();
            assetManager.GetAllLoadedAssets().Returns(Array.Empty<IAsset>());
            
            var splitter = new PreviewSplitter(memoryManager, assetManager, memoryPerSetLocationMb, memoryPerCharacterMb, memoryForOthersMb, memoryForUmaBuildProcess);

            const long setLocationId = 100;
            const long characterId = 300;
            var level = new Level();
            for (var i = 0; i < 10; i++)
            {
                var ev = new Event();
                ev.LevelSequence = i;
                ev.SetLocationController.Add(new SetLocationController()
                {
                    SetLocationId = setLocationId
                });

                for (var n = 0; n < charactersPerEvent; n++)
                {
                    ev.CharacterController.Add(new CharacterController
                    {
                        CharacterId = characterId,
                        Character = new CharacterFullInfo {Id = characterId}
                    });
                }
                
                level.Event.Add(ev);
            }

            //action
            var piece = splitter.GetNextPiece(level.Event, SplitType.KeepAssetsInRamFromOneEventMax);

            //assert
            Assert.IsTrue(piece.Events.Length == level.Event.Count);
        }
    }
}