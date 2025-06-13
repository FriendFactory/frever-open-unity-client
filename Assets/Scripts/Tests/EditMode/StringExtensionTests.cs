using Extensions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Tests.EditMode
{
    [UsedImplicitly]
    internal sealed class StringExtensionTests
    {
        [Test]
        [TestCase("012345", new []{4}, "0123\n45")]
        [TestCase("012345", new []{1, 4}, "0\n123\n45")]
        [TestCase("012345", new []{1, 4, 5}, "0\n123\n4\n5")]
        [TestCase("0\n12345", new []{1}, "0\n12345")]
        public void InsertNewLines_OutputShouldHaveNewLinesAtTargetIndexes(string targetString, int[] indexesToInsert, string expectedResult)
        {
            //act
            var result = targetString.InsertNewLinesIfNotAdded(indexesToInsert);
            //assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}