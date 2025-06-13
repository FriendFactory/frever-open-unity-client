using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Full;
using Models;

namespace Extensions.ResetEntity
{
    internal sealed class ResetAlgorithmProvider
    {
        private readonly Dictionary<Type, ResetAlgorithm> _algorithms;
        private readonly ResetAlgorithm _default = new SimplestResetMainIdAlgorithm();

        public ResetAlgorithmProvider()
        {
            _algorithms = new Dictionary<Type, ResetAlgorithm>();
            SetupAlgorithms();
        }

        public ResetAlgorithm GetAlgorithm(Type targetType)
        {
            return _algorithms.TryGetValue(targetType, out var algorithm) ? algorithm : _default;
        }

        private void SetupAlgorithms()
        {
            //types, should have reset id when we reset level
            var levelTreeTypes = new[]
            {
                typeof(Event),
                typeof(Level),
                typeof(FaceAnimationFullInfo),
                typeof(VoiceTrackFullInfo),
                typeof(CameraAnimationFullInfo),
                typeof(CharacterControllerFaceVoice),
                typeof(CharacterController),
                typeof(CameraController),
                typeof(VfxController),
                typeof(SetLocationController),
                typeof(MusicController),
                typeof(CharacterControllerBodyAnimation),
                typeof(CameraFilterController),
                typeof(VideoClipFullInfo),
                typeof(PhotoFullInfo),
                typeof(CaptionFullInfo)
            };
            
            foreach (var type in levelTreeTypes)
            {
                AddAlgorithm(type, levelTreeTypes);
            }
        }

        private void AddAlgorithm(Type modelType, Type[] childTypes)
        {
            var genericAlgType = typeof(GenericResetAlgorithm<>).MakeGenericType(modelType);
            var algInstance = Activator.CreateInstance(genericAlgType, (object) childTypes);
            _algorithms.Add(modelType, algInstance as ResetAlgorithm);
        }
    }
}