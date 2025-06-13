using System;
using Extensions;
using NUnit.Framework;

namespace Tests.EditMode
{
    internal sealed class ColorExtensionTest
    {
        [Test]
        [TestCase("#FFFFFF")]
        [TestCase("#ffffff")]
        [TestCase("#F95CAC")]
        [TestCase("#F8783C")]
        [TestCase("#E9C91E")]
        [TestCase("#52E552")]
        [TestCase("#001aff")]
        public void ConvertHexToUnityColorAndBack_ShouldReturnTheSameHexAsInitially(string inputHex)
        {
            var color = ColorExtension.HexToColor(inputHex);
            var resultHex = color.ToHexRgb();
            Assert.IsTrue(string.Equals(resultHex, inputHex, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}