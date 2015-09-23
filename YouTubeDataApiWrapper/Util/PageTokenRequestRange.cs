using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeDataRetrievalWrapper.Util
{
   /// <summary>
   /// Provides a enumerable of <see cref="PageTokenObject"/> based on parameters discribing how many items to get.
   /// Access the PageTokens through parameter <see cref="PageTokens"/>
   /// </summary>
   /// <remarks>
   /// note the actual number of items returned
   /// may be anything less than <see cref="RequestRange.NumberOfItems"/>, or upto <see cref="RequestRange.MaxResultsPerPage"/> minus 1, more
   /// </remarks>
    public class PageTokenRequestRange : RequestRange
    {
        public readonly IEnumerable<PageTokenObject> PageTokens;
        private readonly PageTokenGenerator _pageTokenGenerator = new PageTokenGenerator();

        /// <summary>
        /// Creates default range with startIndex as 0 and MaxResultsPerPage as 50
        /// </summary>
        /// <param name="numberOfItems">the total number of items, <remarks>should be a multiple of MaxResultsPerPage, note you are not guranteed this number of items will be returned.</remarks></param>
        public PageTokenRequestRange(int numberOfItems)
            : this(0, numberOfItems)
        {
        }

        /// <summary>
        /// Creates custom range
        /// </summary>
        /// <param name="startIndex">Index of first item</param>
        /// <param name="numberOfItems">the total number of items following startIndex, <remarks>should be a multiple of MaxResultsPerPage, note you are not guranteed this number of items will be returned.</remarks></param>
        /// <param name="maxResultsPerPage">integer between 0 and 50 inclusive</param>
        public PageTokenRequestRange(int startIndex, int numberOfItems, int maxResultsPerPage = 50)
            : base(startIndex, numberOfItems, maxResultsPerPage)
        {
            this.PageTokens = PageTokensEnumerable();
        }

        private IEnumerable<PageTokenObject> PageTokensEnumerable()
        {
            for (var i = 0; i < (Math.Ceiling((double)NumberOfItems / MaxResultsPerPage)); i++)
            {
                yield return _pageTokenGenerator.GetTokenObject((i * MaxResultsPerPage) + StartIndex);
            }
        }
    }
}
