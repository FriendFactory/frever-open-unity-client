using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraAnimations;
using UnityEngine;

namespace Modules.CameraSystem.CameraSystemCore
{
    /// <summary>
    /// Provides method for replacing camera animation properties from one animation to another
    /// Generates a new animation as a result
    /// It was made to be able updated only postprocessing camera properties, not related to movement (such as depth of field or field of view),
    /// because camera path regeneration process is not perfect(can lead to small deviations).
    /// </summary>
    [UsedImplicitly]
    internal sealed class CameraAnimationPropsReplacer
    {
        private readonly CameraAnimationConverter _cameraAnimationConverter;
        private readonly CameraAnimationSaver _cameraAnimationSaver;

        public CameraAnimationPropsReplacer(CameraAnimationConverter cameraAnimationConverter, CameraAnimationSaver cameraAnimationSaver)
        {
            _cameraAnimationConverter = cameraAnimationConverter;
            _cameraAnimationSaver = cameraAnimationSaver;
        }

        public CameraAnimationSavingResult ReplaceProperties(RecordedCameraAnimationClip originAnimation,
            string sourceForReplacingValuesAnimationString, CameraAnimationProperty[] propertiesToReplace)
        {
            var sourceForReplacing = _cameraAnimationConverter.ConvertToClipData(sourceForReplacingValuesAnimationString);
            var newAnimationProps = new Dictionary<CameraAnimationProperty, AnimationCurve>();
            foreach (var originAnimData in originAnimation.AnimationCurves)
            {
                var targetProperty = originAnimData.Key;
                newAnimationProps[targetProperty] = propertiesToReplace.Contains(targetProperty)
                    ? sourceForReplacing[targetProperty]
                    : originAnimData.Value;
            }

            var timeLine = originAnimation.AnimationCurves.First().Value.keys.Select(x => x.time).ToList();
            var newAnimationPropsValues =
                newAnimationProps.ToDictionary(x => x.Key, x => (IList<float>)x.Value.keys.Select(keyframe => keyframe.value).ToList());
            var newAnimationString = _cameraAnimationConverter.ConvertToString(timeLine, newAnimationPropsValues);
            var path = _cameraAnimationSaver.SaveTextFileFromString(newAnimationString);
            return new CameraAnimationSavingResult(path, newAnimationString);
        }
    }
}