using System;
using System.Collections.Generic;
using Extensions;

namespace Modules.FaceAndVoice.Face.Core
{
    public sealed class FaceBlendShapeMap
    {
        private const string BLEND_SHAPE_PREFIX = "blendShape1.";
        
        private readonly Dictionary<int, string> _blendShapeNames = new Dictionary<int, string>();

        private readonly Dictionary<string, BlendShape> _blendShapeNameToBlendShapeEnum =
            new Dictionary<string, BlendShape>();
        
        public int GetBlendShapeId(BlendShape blendShape)
        {
            return (int) blendShape;
        }

        public string GetBlendShapeName(BlendShape blendShape)
        {
            var blendShapeId = GetBlendShapeId(blendShape);
            return GetBlendShapeName(blendShapeId);
        }

        public string GetBlendShapeName(int blendShapeId)
        {
            if (!_blendShapeNames.ContainsKey(blendShapeId))
            {
                var blendShape = GetBlendShapeById(blendShapeId);
                var blendShapeName = $"{BLEND_SHAPE_PREFIX}{blendShape.ToString().FirstCharToLower()}";
                _blendShapeNames.Add(blendShapeId, blendShapeName);
                _blendShapeNameToBlendShapeEnum.Add(blendShapeName, blendShape);
            }

            return _blendShapeNames[blendShapeId];
        }

        public BlendShape GetBlendShapeById(int id)
        {
            return (BlendShape) id;
        }

        public bool IsFaceBlendShape(string blendShapeName)
        { 
            TryRegisterFaceBlendShape(blendShapeName);
            return _blendShapeNameToBlendShapeEnum.ContainsKey(blendShapeName);
        }
        
        public BlendShape GetBlendShapeByName(string blendShapeName)
        {
            if (!IsFaceBlendShape(blendShapeName))
            {
                throw new InvalidOperationException("Trying to get face blend shape enum from non face blend shape");
            }
            
            return _blendShapeNameToBlendShapeEnum[blendShapeName];
        }

        private bool TryRegisterFaceBlendShape(string blendShapeName)
        {
            if (_blendShapeNameToBlendShapeEnum.ContainsKey(blendShapeName))
                return true;

            var blendShapeEnumName = blendShapeName.Replace(BLEND_SHAPE_PREFIX, string.Empty).FirstCharToUpper();
            if(Enum.TryParse(blendShapeEnumName, out BlendShape blendShape))
            {
                _blendShapeNameToBlendShapeEnum.Add(blendShapeName, blendShape);
                return true;
            }

            return false;
        }
    }
}