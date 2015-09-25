using System;

namespace YouTubeDataApiWrapper.Util
{
    public class RequestRange
    {
        public readonly int MaxResultsPerPage;
        public readonly int NumberOfItems;
        public readonly int StartIndex;

        public RequestRange(int numberOfItems) : this(0, numberOfItems)
        {
        }

        protected RequestRange(int startIndex, int numberOfItems, int maxResultsPerPage = 50)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "startIndex must be >= 0");
            if (numberOfItems < 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfItems), numberOfItems, "numberOfItems must be > 0");
            if (maxResultsPerPage < 1 || maxResultsPerPage > 50)
                throw new ArgumentOutOfRangeException(nameof(maxResultsPerPage), maxResultsPerPage,
                    "MaxResultsPerPage must be between 1 and 50 inclusive");

            StartIndex = startIndex;
            NumberOfItems = numberOfItems;
            MaxResultsPerPage = maxResultsPerPage;
        }
    }
}
