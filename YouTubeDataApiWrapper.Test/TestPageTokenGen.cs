using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YouTubeDataRetrievalWrapper.Util;

namespace YouTubeDataRetrievalWrapperTesting
{
    [TestClass]
    public class TestPageTokenGenerator
    {
        public PageTokenGenerator PageTokenGen = new PageTokenGenerator();
        public TupleList<string, int> TokenIndexPairs = new TupleList<string, int>
        {
            {"CAAQAA", 0},
            {"CH8QAA", 127},
            {"CIABEAA", 128},
            {"CIEBEAA", 129},
            {"CP8_EAA", 8191},
            {"CIBAEAA", 8192},
            {"CIFAEAA", 8193},
            {"CP9_EAA", 16383},
            {"CICAARAA", 16384},
            {"CIGAARAA", 16385},
            {"CP-_ARAA", 24575},
            {"CIDAARAA", 24576},
            {"CIHAARAA", 24577},
            {"CP__ARAA", 32767},
            {"CICAAhAA", 32768},
            {"CIGAAhAA", 32769},
            {"CP-_AhAA", 40959},
            {"CIDAAhAA", 40960},
            {"CIHAAhAA", 40961},
            {"CP__AhAA", 49151},
            {"CICAAxAA", 49152},
            {"CIGAAxAA", 49153},
            {"CP-_AxAA", 57343},
            {"CIDAAxAA", 57344},
            {"CIHAAxAA", 57345},
            {"CP__AxAA", 65535},
            {"CICABBAA", 65536},
            {"CIGABBAA", 65537},
            {"CP-_BBAA", 73727},
            {"CIDABBAA", 73728},
            {"CIHABBAA", 73729},
            {"CP__BBAA", 81919},
            {"CICABRAA", 81920},
            {"CIGABRAA", 81921},
            {"CP-_BRAA", 90111},
            {"CIDABRAA", 90112},
            {"CIHABRAA", 90113},
            {"CP__BRAA", 98303},
            {"CICABhAA", 98304},
            {"CIGABhAA", 98305},
            {"CJ-NBhAA", 99999},
            {"CP-_BhAA", 106495},
            {"CIDABhAA", 106496},
            {"CIHABhAA", 106497},
            {"CP__BhAA", 114687},
            {"CICABxAA", 114688},
            {"CIGABxAA", 114689},
            {"CIDABxAA", 122880},
            {"CICACBAA", 131072},
            {"CICAChAA", 163840},
            {"CICACxAA", 180224},
            {"CIDADBAA", 204800},
            {"CICADRAA", 212992},
            {"CIDADRAA", 221184},
            {"CICADhAA", 229376},
            {"CICAQRAA", 1064960}
        };

        [TestMethod]
        public void TokensShouldBeGeneratedAsPerTheTokenIndexPairs()
        {
            foreach (var tokenIndexPair in TokenIndexPairs)
            {
                Assert.AreEqual(tokenIndexPair.Item1, PageTokenGen.NumberToToken(tokenIndexPair.Item2));
            }
        }

        [TestMethod]
        public void NumbersShouildBeGeneratedAsPerTheTokenIndexPairs()
        {
            foreach (var tokenIndexPair in TokenIndexPairs)
            {
                Assert.AreEqual(PageTokenGen.TokenToNumber(tokenIndexPair.Item1), tokenIndexPair.Item2);
            }
        }

        [TestMethod]
        public void TokensShouldBeGeneratedCorrectlyForPrevPageTokens()
        {
            foreach (var tokenIndexPair in TokenIndexPairs)
            {
                var asPrevToken = tokenIndexPair.Item1.Remove(tokenIndexPair.Item1.Length - 1) + "_";
                Assert.AreEqual(asPrevToken, PageTokenGen.NumberToPrevToken(tokenIndexPair.Item2));
            }
        }

        [TestMethod]
        public void NumbersShouldBeGeneratedCorrectlyForPrevPageTokens()
        {
            foreach (var tokenIndexPair in TokenIndexPairs)
            {
                var asPrevToken = tokenIndexPair.Item1.Remove(tokenIndexPair.Item1.Length - 1) + "_";
                Assert.AreEqual(PageTokenGen.TokenToNumber(asPrevToken), tokenIndexPair.Item2);
            }
        }

        [TestMethod]
        public void ConvertingBetweenPageTokenAndPrevPageTokenAndIntegerRepresentationsShouldNotChangeTheResult()
        {
            foreach (var tokenIndexPair in TokenIndexPairs)
            {
                var prevToken = PageTokenGen.NumberToPrevToken(tokenIndexPair.Item2);
                var num1 = PageTokenGen.TokenToNumber(prevToken);
                var nexToken = PageTokenGen.NumberToToken(num1);
                Assert.AreEqual(tokenIndexPair.Item2, PageTokenGen.TokenToNumber(nexToken));
            }
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException), "Not A Valid PageToken")]
        public void InvalidToken_TokenTooShort()
        {
            var x = PageTokenGen.TokenToNumber("CAAQ");
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Not A Valid PageToken")]
        public void InvalidToken_InvalidCharacters()
        {
            var x =PageTokenGen.TokenToNumber("CA+QAA");
        }

        [TestMethod]
        public void NegativeTokenIndexReturnsZeroToken()
        {
            var token = PageTokenGen.NumberToToken(-1);
            Assert.AreEqual("CAAQAA", token);
        }

    }
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }
}

//[TestMethod]
//public void ConvertingFromPageTokenToNumberAndBackShouldNotChangeTheResult()
//{
//    foreach (var tokenIndexPair in TokenIndexPairs)
//    {
//        var num = PageTokenGen.TokenToNumber(tokenIndexPair.Item1);
//        Assert.AreEqual(tokenIndexPair.Item1, PageTokenGen.NumberToToken(num));
//    }

//    foreach (var tokenIndexPair in TokenIndexPairs)
//    {
//        var asPrevToken = tokenIndexPair.Item1.Remove(tokenIndexPair.Item1.Length - 1) + "_";
//        var num = PageTokenGen.TokenToNumber(asPrevToken);
//        Assert.AreEqual(asPrevToken, PageTokenGen.NumberToPrevToken(num));
//    }

//}

//[TestMethod]
//public void ConvertingFromNumberToPreviousPageTokenToNumberToNextPageTokenToNumberShouldNotChangeTheResult()
//{
//    foreach (var tokenIndexPair in TokenIndexPairs)
//    {
//        var prevToken = PageTokenGen.NumberToPrevToken(tokenIndexPair.Item2);
//        var num1 = PageTokenGen.TokenToNumber(prevToken);
//        var nexToken = PageTokenGen.NumberToToken(num1);
//        Assert.AreEqual(tokenIndexPair.Item2, PageTokenGen.TokenToNumber(nexToken));
//    }
//}