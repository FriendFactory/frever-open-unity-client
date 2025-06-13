using System.Collections.Generic;
using System.Linq;
using Modules.CameraSystem.CameraAnimations;
using NUnit.Framework;

namespace Tests.EditMode
{
    internal sealed class RotationCurve360FlipCorrectorTest
    {
        private readonly RotationCurve360FlipCorrector _curve360FlipCorrector = new RotationCurve360FlipCorrector();
        
        [Test]
        //should convert flip values from 5,6,7 degrees to 365, 366, 367...
        public void CorrectForwardDirectionCurve()
        {
            //Arrange
            var curveValues = new List<float>();

            for (var i = 340; i <= 355; i++)
            {
                curveValues.Add(i);
            }
            
            //add flip 355 -> 5 degrees
            for (var i = 5; i < 40; i++)
            {
                curveValues.Add(i);
            }
            
            //Act
            var hasFlip = _curve360FlipCorrector.Has360Flip(curveValues);
            if (hasFlip)
            {
                _curve360FlipCorrector.FixFlip(curveValues);
            }

            //Assert
            Assert.True(hasFlip);
            Assert.That(curveValues, Is.Ordered.Ascending);
            Assert.True(curveValues.Any(x=>x > 360));
        }
        
        [Test]
        //should convert flip values from 365, 366, 366  degrees to -5, -6, -7
        public void CorrectBackwardDirectionCurve()
        {
            //Arrange
            var curveValues = new List<float>();

            for (var i = 40; i >= 5; i--)
            {
                curveValues.Add(i);
            }
            
            //add flip 5 -> 365 degrees
            for (var i = 365; i > 340; i--)
            {
                curveValues.Add(i);
            }
            
            //Act
            var hasFlip = _curve360FlipCorrector.Has360Flip(curveValues);
            if (hasFlip)
            {
                _curve360FlipCorrector.FixFlip(curveValues);
            }

            //Assert
            Assert.True(hasFlip);
            Assert.That(curveValues, Is.Ordered.Descending);
            Assert.True(curveValues.Any(x=>x < 0));
        }
        
        [Test]
        //should convert flip values from 5,6,7 degrees to 365, 366, 367...
        //and when animation reverse direction should convert vice versa
        public void CorrectReverseDirectionCurve()
        {
            //Arrange
            var curveValues = new List<float>();

            for (var i = 340; i <= 355; i++)
            {
                curveValues.Add(i);
            }
            
            //add first flip from 355 -> 5 degrees
            for (var i = 5; i < 40; i++)
            {
                curveValues.Add(i);
            }
            
            //reverse back
            for (var i = 40; i > 5; i--)
            {
                curveValues.Add(i);
            }
            for (var i = 355; i > 340; i--)
            {
                curveValues.Add(i);
            }
            
            //Act
            var hasFlip = _curve360FlipCorrector.Has360Flip(curveValues);
            if (hasFlip)
            {
                _curve360FlipCorrector.FixFlip(curveValues);
            }

            //Assert
            Assert.True(hasFlip);
            var forwardAnimPart = curveValues.GetRange(0, curveValues.Count / 2);
            var secondPartIndex = curveValues.Count / 2 + 1;
            var reverseAnimPart = curveValues.GetRange(secondPartIndex, curveValues.Count - secondPartIndex);
            Assert.That(forwardAnimPart, Is.Ordered.Ascending);
            Assert.That(reverseAnimPart, Is.Ordered.Descending);
            Assert.True(curveValues.Any(x=>x > 360));
        }
    }
}