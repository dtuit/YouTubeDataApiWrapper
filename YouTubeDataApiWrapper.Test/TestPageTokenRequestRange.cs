using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YouTubeDataApiWrapper.Util;

namespace YouTubeDataApiWrapper.Test
{
    [TestClass]
    public class TestPageTokenRequestRange
    {
        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void InvalidStartIndex()
        {
            var range = new PageTokenRequestRange(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidNumberOfItems()
        {
            var range = new PageTokenRequestRange(0, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidMaxNumberOfItems()
        {
            var range = new PageTokenRequestRange(0, 0, 0);
            //var range2 = new PageTokenRequestRange(0, 0 , 50);
        }

        [TestMethod]
        public void CorrectNumberOfPageTokensAreGenerated()
        {
            for (int i = 1; i <= 50; i++)
            {
                for (int j = 1; j < 150; j++)
                {
                    var maxResults = i;
                    var numberItems = j;
                    var range = new PageTokenRequestRange(0, numberItems, maxResults);
                    Assert.AreEqual(range.PageTokens.Count(), calcNumberofPageTokens(numberItems, maxResults));

                }
            }

           
        }

        private int calcNumberofPageTokens(int numItems, int maxResults)
        {
            return (int) Math.Ceiling((double)numItems/ maxResults);
        }
    }
}
