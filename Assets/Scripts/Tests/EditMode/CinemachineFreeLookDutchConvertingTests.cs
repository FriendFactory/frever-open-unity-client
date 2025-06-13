using Modules.CameraSystem.Extensions.CinemachineExtensions;
using NUnit.Framework;

namespace Tests.EditMode
{
    internal sealed class CinemachineFreeLookDutchConvertingTests
    {
        [Test]
        [TestCase(0, 0)]
        [TestCase(50, 50)]
        [TestCase(180, 180)]
        [TestCase(-30, -30)]
        [TestCase(327, -33)]
        [TestCase(507, 147)]
        [TestCase(687, -33)]
        public void ConvertAngleZToDutch(float angleZ, float expectedDutch)
        {
            //act
            var actualDutch = CinemachineFreeLookUtils.EulerAngleZToDutch(angleZ);
            
            //assert
            Assert.AreEqual(actualDutch, expectedDutch, 0.0001f);
        }
    }
}
