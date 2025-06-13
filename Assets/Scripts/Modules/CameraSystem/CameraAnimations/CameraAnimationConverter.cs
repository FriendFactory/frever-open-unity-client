using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations
{
    public sealed class CameraAnimationConverter
    {
        private const string DELIMITER_SIGN = "|";
        private const string NEW_LINE_SIGN = "\n";
        private const string OPTIMIZED_STORE_FORMAT = "0.###";
        private const string BEST_ACCURACY_STORE_FORMAT = "0.#######";
        private const int TIME_POSITION_INDEX = 0;
        
        private static readonly CameraAnimationProperty[] StoreWithOptimizedFormat =
        {
            CameraAnimationProperty.DepthOfField,
            CameraAnimationProperty.FocusDistance,
            CameraAnimationProperty.FieldOfView
        };
        private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;
        private static readonly StringBuilder StringBuilder = new StringBuilder();

        private static readonly CameraAnimationProperty[] AllCameraAnimationProperties =
            Enum.GetValues(typeof(CameraAnimationProperty)).Cast<CameraAnimationProperty>().ToArray();

        public string ConvertToString(IList<float> timeLine, IDictionary<CameraAnimationProperty, IList<float>> propertiesCurves)
        {
            StringBuilder.Clear();

            var totalCurvesCount = 1 + propertiesCurves.Count;//time + camera animation properties
            var totalFrames = timeLine.Count;
            
            for (var rowIndex = 0; rowIndex < totalFrames; rowIndex++)
            for (var columnIndex = 0; columnIndex < totalCurvesCount; columnIndex++)
            {
                string valueString;
                var isTimeColumn = columnIndex == TIME_POSITION_INDEX;
                if (isTimeColumn)
                {
                    valueString = timeLine[rowIndex].ToString(BEST_ACCURACY_STORE_FORMAT, CultureInfo);
                    StringBuilder.Append(valueString);
                    StringBuilder.Append(DELIMITER_SIGN);
                    continue;
                }

                var cameraProperty = (CameraAnimationProperty) (columnIndex - 1);
                var cameraPropCurve = propertiesCurves[cameraProperty];
                var format = StoreWithOptimizedFormat.Contains(cameraProperty)
                    ? OPTIMIZED_STORE_FORMAT
                    : BEST_ACCURACY_STORE_FORMAT;

                valueString = cameraPropCurve[rowIndex].ToString(format, CultureInfo);
                StringBuilder.Append(valueString);
                StringBuilder.Append(columnIndex == totalCurvesCount - 1 ? NEW_LINE_SIGN : DELIMITER_SIGN);
            }
            
            return StringBuilder.ToString();
        }

        public IDictionary<CameraAnimationProperty, AnimationCurve> ConvertToClipData(string animationString)
        {
            if (string.IsNullOrEmpty(animationString)) throw new ArgumentNullException(nameof(animationString));

            var output = new Dictionary<CameraAnimationProperty, AnimationCurve>();
            var frames = SplitToFrameStrings(animationString);

            foreach (var frameString in frames)
            {
                ReadFrameProperties(frameString, AddToAnimationCurves);
            }

            void AddToAnimationCurves(PropertyData propertyData)
            {
                var property = propertyData.Property;
                if (!output.ContainsKey(property)) output.Add(property, new AnimationCurve());
                output[property].AddKey(propertyData.Time, propertyData.Value);
            }
            
            return output;
        }

        public CameraAnimationFrame ExtractLastFrame(string animationString)
        {
            if (string.IsNullOrEmpty(animationString)) throw new ArgumentNullException(nameof(animationString));

            var frameRows = SplitToFrameStrings(animationString);
            var lastFrame = frameRows.Last();
            var output = new Dictionary<CameraAnimationProperty, float>();
            ReadFrameProperties(lastFrame, SavePropertyValues);
            
            void SavePropertyValues(PropertyData propData)
            {
                output[propData.Property] = propData.Value;
            }   
            return new CameraAnimationFrame(output);
        }

        private string[] SplitToFrameStrings(string animationString)
        {
            return animationString.Split('\n').Where(x=>!string.IsNullOrEmpty(x)).ToArray();
        }
        
        private void ReadFrameProperties(string frameString, Action<PropertyData> onPropertyRead)
        {
            var element = frameString.Split('|');
            var time = float.Parse(element[TIME_POSITION_INDEX], CultureInfo.InvariantCulture);

            foreach (var property in AllCameraAnimationProperties)
            {
                var propertyPositionIndex = (int)property + 1;//index 0 is for the time
                if (element.Length <= propertyPositionIndex) break;
                
                var textValue = element[propertyPositionIndex];
                if (string.IsNullOrWhiteSpace(textValue)) continue;
                
                var value = float.Parse(textValue, CultureInfo.InvariantCulture);
                var propData = new PropertyData
                {
                    Property = property,
                    Time = time,
                    Value = value
                };
                onPropertyRead.Invoke(propData);
            }
        }

        private struct PropertyData
        {
            public CameraAnimationProperty Property;
            public float Time;
            public float Value;
        }
    }
}
