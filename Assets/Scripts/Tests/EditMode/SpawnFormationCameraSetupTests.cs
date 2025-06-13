using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using Modules.LevelManaging.Editing.CameraManaging.CameraSpawnFormation;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    internal sealed class SpawnFormationCameraSetupTests
    {
        [Test]
        [TestCase(FormationType.DuoStory, 1, -1)]
        [TestCase(FormationType.TrioDance, 0, -1)]
        [TestCase(FormationType.TrioQueue, 2, -1)]
        public void MakeTestSetupCollectionWithAllFormations_FindTarget_ShouldReturnAngleFromProperSettings(FormationType targetFormationType, int characterSequenceNumber, float defaultAngle)
        {
            //arrange
            var setupCollection = GetCameraSetupForAllSpawnFormations(100);

            var targetSetup = setupCollection.First(x => x.FormationType == targetFormationType);
            var targetSettings =
                targetSetup.CameraSettings.First(x => x.CharacterSequenceNumber == characterSequenceNumber);
            var expected = targetSettings.ViewAngle * Mathf.Sign((int)targetSettings.ViewDirection);

            var angleProvider = new SpawnFormationCameraAngleProvider(setupCollection, defaultAngle);
            
            //act
            var result = angleProvider.GetAngle(targetFormationType.GetId(), characterSequenceNumber);
            
            //assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(FormationType.DuoStory, 1, 10)]
        [TestCase(FormationType.TrioDance, 0, 20)]
        [TestCase(FormationType.TrioQueue, 2, 30)]
        public void PrepareCollectionWithNotAllFormationSetups_TryToGetAngleForMissed_ShouldReturnDefaultValue(FormationType targetFormation, int characterSequence, float defaultAngle)
        {
            //arrange
            var setupCollection = GetCameraSetupForAllSpawnFormations(100);

            var targetFormationTypeCameraSetup = setupCollection.First(x => x.FormationType == targetFormation);
            var targetCameraSettings =
                targetFormationTypeCameraSetup.CameraSettings.First(x =>
                    x.CharacterSequenceNumber == characterSequence);
            targetFormationTypeCameraSetup.CameraSettings.Remove(targetCameraSettings);
            
            var angleProvider = new SpawnFormationCameraAngleProvider(setupCollection, defaultAngle);
            var expected = defaultAngle;
            
            //act
            var result = angleProvider.GetAngle(targetFormation.GetId(), characterSequence);

            //assert
            Assert.AreEqual(expected, result);
        }
        
        [Test]
        [TestCase(FormationType.DuoStory, 10)]
        [TestCase(FormationType.TrioDance, 20)]
        [TestCase(FormationType.TrioQueue, 30)]
        public void PrepareCollectionWithNotAllFormationSetups_TryToGetAngleForGroupFocus_ShouldReturnDefaultValue(FormationType targetFormation, float defaultAngle)
        {
            //arrange
            const int groupFocusSequenceNumber = -1;
            
            var setupCollection = GetCameraSetupForAllSpawnFormations(100);
            var angleProvider = new SpawnFormationCameraAngleProvider(setupCollection, defaultAngle);
            var expected = defaultAngle;
            
            //act
            var result = angleProvider.GetAngle(targetFormation.GetId(), groupFocusSequenceNumber);

            //assert
            Assert.AreEqual(expected, result);
        }

        private List<SpawnFormationCameraData> GetCameraSetupForAllSpawnFormations(float smallestStartViewAngle)
        {
            var setupCollection = new List<SpawnFormationCameraData>();
            foreach (FormationType formationType in Enum.GetValues(typeof(FormationType)))
            {
                var setup = new SpawnFormationCameraData
                {
                    FormationType = formationType,
                    CameraSettings = new List<CameraSettings>()
                };
                setupCollection.Add(setup);
                
                for (var i = 0; i < Constants.CHARACTERS_IN_SPAWN_FORMATION_MAX; i++)
                {
                    var settings = new CameraSettings
                    {
                        CharacterSequenceNumber = i,
                        ViewAngle = smallestStartViewAngle++
                    };
                    setup.CameraSettings.Add(settings);
                }
            }

            return setupCollection;
        }
    }
}