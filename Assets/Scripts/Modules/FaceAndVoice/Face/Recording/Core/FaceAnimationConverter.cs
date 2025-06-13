using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Modules.FaceAndVoice.Face.Core;

namespace Modules.FaceAndVoice.Face.Recording.Core
{
    /// <summary>
    /// Converts face animation to string as vise versa
    /// </summary>
    public sealed class FaceAnimationConverter
    {
        private const char BLEND_SHAPES_DELIMITER = ',';
        private const char TIME_DELIMITER = '#';
        private const char BLEND_SHAPE_VALUE_DELIMITER = ':';
        private const char FRAMES_DELIMITER = '\n';
        
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly FaceBlendShapeMap _faceBlendShapeMap;

        public FaceAnimationConverter(FaceBlendShapeMap faceBlendShapeMap)
        {
            _faceBlendShapeMap = faceBlendShapeMap;
        }

        public Task<string> ConvertToStringAsync(FaceAnimationData faceAnimationData)
        {
            return Task.Run(() => ConvertToString(faceAnimationData));
        }
        
        public string ConvertToString(FaceAnimationData faceAnimationData)
        {
            _stringBuilder.Clear();
            var frameCounter = 0;
            foreach (var frame in faceAnimationData.Frames)
            {
                _stringBuilder.Append(frame.FrameTime.ToString(CultureInfo.InvariantCulture));
                _stringBuilder.Append(TIME_DELIMITER);
                AppendBlendShapesValues(frame, _stringBuilder);
                frameCounter++;
                var isLastFrame = frameCounter == faceAnimationData.Frames.Count;
                if(isLastFrame) break;
                
                _stringBuilder.Append(FRAMES_DELIMITER);
            }

            var output = _stringBuilder.ToString();
            _stringBuilder.Clear();
            return output;
        }

        public FaceAnimationData ConvertToFaceAnimationData(string faceAnimText)
        {
            var framesData = faceAnimText.Split(FRAMES_DELIMITER);
            var frames = new List<FaceAnimFrame>(framesData.Length);
            foreach (var frameData in framesData)
            {
                var frame = ParseFrame(frameData);
                frames.Add(frame);
            }
            return new FaceAnimationData(frames);
        }

        private void AppendBlendShapesValues(FaceAnimFrame frame, StringBuilder stringBuilder)
        {
            var blendShapeCounter = frame.BlendShapesData.Count;
            foreach (var blendShapeData in frame.BlendShapesData)
            {
                var blendShapeId = _faceBlendShapeMap.GetBlendShapeId(blendShapeData.Key);
                stringBuilder.AppendFormat("{0}{1}{2}", blendShapeId, BLEND_SHAPE_VALUE_DELIMITER, blendShapeData.Value.ToString(CultureInfo.InvariantCulture));
                blendShapeCounter--;
                var isLastValue = blendShapeCounter == 0;
                if (!isLastValue)
                {
                    stringBuilder.Append(BLEND_SHAPES_DELIMITER);
                }
            }
        }

        private FaceAnimFrame ParseFrame(string frameText)
        {
            float frameTime = 0;
            var blendShapes = new Dictionary<BlendShape, float>();
            int nextValueStartIndex = 0;
            
            for (var i = 0; i < frameText.Length; i++)
            {
                var currentChar = frameText[i];
                if (currentChar == TIME_DELIMITER)
                {
                    var timeText = frameText.Substring(0, i);
                    frameTime = float.Parse(timeText, CultureInfo.InvariantCulture);
                    nextValueStartIndex = i + 1;
                    continue;
                }

                var isBlendShapesDelimiter = currentChar == BLEND_SHAPES_DELIMITER;
                if (isBlendShapesDelimiter || i == frameText.Length-1)
                {
                    var blendShapeTextLastIndex = isBlendShapesDelimiter ? i - 1: i;//exclude delimiter sign
                    var blendShapeText = frameText.Substring(nextValueStartIndex, blendShapeTextLastIndex - nextValueStartIndex + 1);//include first char
                    var delimiterIndex = blendShapeText.IndexOf(BLEND_SHAPE_VALUE_DELIMITER);
                    var blendShapeIdText = blendShapeText.Substring(0, delimiterIndex);
                    var blendShapeId = int.Parse(blendShapeIdText);

                    var blendShapeValueText =
                        blendShapeText.Substring(delimiterIndex + 1, blendShapeText.Length - delimiterIndex - 1);
                    var blendShapeValue = float.Parse(blendShapeValueText, CultureInfo.InvariantCulture);

                    var blendShape = _faceBlendShapeMap.GetBlendShapeById(blendShapeId);
                    blendShapes.Add(blendShape, blendShapeValue);

                    nextValueStartIndex = i + 1;
                }
            }

            return new FaceAnimFrame(frameTime, blendShapes);
        }
    }
}