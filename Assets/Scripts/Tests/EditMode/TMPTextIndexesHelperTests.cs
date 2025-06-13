using System.Linq;
using Extensions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Tests.EditMode
{
    [UsedImplicitly]
    internal sealed class TMPTextIndexesHelperTests
    {
        [Test]
        [TestCase("text without emojies should show the same indexes", new []{3}, new []{3})]
        [TestCase("text with 2-char emoji \ud83d\ude11 should have different", new []{30}, new []{31})]
        [TestCase("text with 1-char emoji \u2728 should have the same index", new []{30}, new []{30})]
        //the string that contains only 2-char emojies, expected result: tmp indexes multiplied by 2
        [TestCase("\ud83d\ude42\ud83d\ude42\ud83d\ude42\ud83d\ude42\ud83d\ude42\ud83d\ude42\ud83d\ude42\ud83d\ude42\ud83d\ude42\ud83d\ude42", new []{3, 8}, new []{6, 16})] 
        public static void ConvertTMPCharacterInfoIndexesToStringIndexes(string text, int[] tmpInfoIndexes, int[] expectedResult)
        {
            //act
            var result = TMPTextIndexesHelper.ConvertTMPCharacterInfoToStringIndexes(text, tmpInfoIndexes);
            //assert
            Assert.IsTrue(result.SequenceEqual(expectedResult));
        }
    }
}