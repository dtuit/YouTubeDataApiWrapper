using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Requests;
using Google.Apis.YouTube.v3;
using YouTubeDataRetrievalWrapper.RequestBuilders;
using YouTubeDataRetrievalWrapper.Util;
using YouTubeDataRetrievalWrapper.Util.GoogleApiExtensions;

namespace YouTubeDataRetrievalWrapper.RequestServices
{
    /// <summary>
    /// A service that Manages the execution of multiple requests using Paged and Concurrent techniques.
    /// </summary>
    /// <typeparam name="TRequest">Type of the request from <see cref="Google.Apis.YouTube.v3"/></typeparam>
    /// <typeparam name="TResponse">Type of response object from <see cref="Google.Apis.YouTube.v3.Data"/></typeparam>
    /// <typeparam name="TResponseItem">Type of a single item that <see cref="TResponse"/> contains </typeparam>
    public class YoutubeListRequestService<TRequest, TResponse, TResponseItem>
        where TRequest : YouTubeBaseServiceRequest<TResponse>, IClientServiceRequest 
        where TResponse : class, IDirectResponseSchema
        where TResponseItem : class, IDirectResponseSchema
    {
        public BaseListRequestBuilder<TRequest, TResponse> ListRequestBuilder { get; set; }

        //private readonly PageTokenGenerator _pageTokenGenerator = new PageTokenGenerator();

        public YoutubeListRequestService(BaseListRequestBuilder<TRequest, TResponse> listRequestBuilder)
        {
            ListRequestBuilder = listRequestBuilder;
        }

        /// <summary>
        /// Executes Requests in a standard paged manner.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResponseItem>> ExecutePagedAsync(RequestRange range,
            CancellationToken cancellationToken)
        {
            var result = new List<TResponseItem>();
            var pageTokenGen = new PageTokenGenerator();

            ListRequestBuilder.MaxResults = range.MaxResultsPerPage;
            var nextPageToken = pageTokenGen.NumberToToken(range.StartIndex);
            while (nextPageToken != null && result.Count < range.NumberOfItems)
            {
                ListRequestBuilder.PageToken = nextPageToken;
                var response =
                    await ListRequestBuilder.CreateRequest().ExecuteAsync(cancellationToken).ConfigureAwait(false);
                var responseItems = response.GetResponseItems<TResponseItem>();
                
                result.AddRange(responseItems);
                nextPageToken = response.GetNextPageToken();
            }
            return result;
        }

        /// <summary>
        /// Gets All objects in the resource.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResponseItem>> ExecutePagedGetAllAsync(CancellationToken cancellationToken)
        {
            var range = new RequestRange(Int32.MaxValue);
            return await ExecutePagedAsync(range, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a set of requests concurrently using <see cref="MultiRequest"/>
        /// </summary>
        /// <param name="range"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResponseItem>> ExecuteConcurrentAsync(PageTokenRequestRange range,
            CancellationToken cancellationToken)
        {
            return
                await
                    CreateAndExecuteMultiRequest(range.PageTokens.GetEnumerator(), cancellationToken)
                        .ConfigureAwait(false);
        }


        /// <summary>
        /// Executes A set of requests as defined by <paramref name="range"></paramref>, concurrently using <see cref="MultiRequest"/>,
        /// Executes the first request to determine how many items are in the resource, then modifys the range acordingly.
        /// <remarks>
        /// if <see cref="range"/> defines a range larger than the total number of possible items range is modified so to remove unessary requests.
        /// </remarks>
        /// </summary>
        /// <param name="range"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResponseItem>> ExecuteConcurrentCheckExpectedAsync(PageTokenRequestRange range,
            CancellationToken cancellationToken)
        {
            var pageTokens = range.PageTokens.GetEnumerator();

            ListRequestBuilder.MaxResults = range.MaxResultsPerPage;
            pageTokens.MoveNext();
            ListRequestBuilder.PageToken = pageTokens.Current.PageToken;

            var firstRequestResponse =
                (TResponse)
                    await ListRequestBuilder.CreateRequest().ExecuteAsync(cancellationToken).ConfigureAwait(false);
            var pageInfo = firstRequestResponse.GetPageInfo();
            var feedTotal = pageInfo.TotalResults;

            // If attempting to get more items than exist in the feed remove the unnecessary requests
            if (range.NumberOfItems > feedTotal)
            {
                range = new PageTokenRequestRange(range.StartIndex, feedTotal.Value, range.MaxResultsPerPage);
                pageTokens = range.PageTokens.GetEnumerator();
            }
            pageTokens.MoveNext();

            var items = await CreateAndExecuteMultiRequest(pageTokens, cancellationToken).ConfigureAwait(false);
            return firstRequestResponse.GetResponseItems<TResponseItem>().Concat(items);
        }

        /// <summary>
        /// Executes multiple requests relating to a single type of request, useful for when more than 50 items items will be requested.
        /// <example> 
        /// Example: When requesting more than 50 videos using <see cref="VideosResource.ListRequest"/> <see cref="items"/> will be the ids for the videos and <see cref="parameterName"/> will be "Id", <see cref="VideosResource.ListRequest.Id"/>
        /// </example>
        /// </summary>
        /// <param name="items">The list of items to request</param>
        /// <param name="parameterName">name of request Property that the items are associated with</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public async Task<IEnumerable<TResponseItem>> ExecuteConcurrentFromParameters(IEnumerable<string> items,
            string parameterName, CancellationToken cancellationToken)
        {
            if (ListRequestBuilder.GetType().GetProperty(parameterName) == null)
                throw new ArgumentException(parameterName + " is not a valid parameterName for " + typeof(TRequest), nameof(parameterName));
          
            var results = new ConcurrentDictionary<int, IEnumerable<TResponseItem>>();
            var multiRequest = new MultiRequest() {RequiresAuth = true};

            var callback = new MultiRequest.OnResponse<TResponse>(
                (content, error, index, message) =>
                {
                    if (error == null)
                    {
                        results.TryAdd(index, content.GetResponseItems<TResponseItem>());
                    }
                });

            foreach (var value in items.Chunk(50).Select(chunk => string.Join(",", chunk))) //each 50 items as csv
            {
                ListRequestBuilder.GetType().GetProperty(parameterName).SetValue(ListRequestBuilder, value);
                multiRequest.Queue(ListRequestBuilder.CreateRequest(), callback);
            }

            await multiRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            return results.OrderBy(k => k.Key).SelectMany(u => u.Value);
        }

        /// <summary>
        /// Creates requests for each pageToken executes all in a <see cref="MultiRequest"/> then orders the result
        /// </summary>
        private async Task<IEnumerable<TResponseItem>> CreateAndExecuteMultiRequest(
            IEnumerator<PageTokenObject> pageTokens, CancellationToken cancellationToken)
        {
            var results = new ConcurrentDictionary<int, IEnumerable<TResponseItem>>();
            var multiRequest = new MultiRequest() {RequiresAuth = true};

            var callback = new MultiRequest.OnResponse<TResponse>(
                (content, error, index, message) =>
                {
                    if (error == null)
                    {
                        results.TryAdd(index, content.GetResponseItems<TResponseItem>());
                    }
                });

            while (pageTokens.MoveNext())
            {
                this.ListRequestBuilder.PageToken = pageTokens.Current.PageToken;
                multiRequest.Queue(ListRequestBuilder.CreateRequest(), callback);
            }

            await multiRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            return results.OrderBy(k => k.Key).SelectMany(x => x.Value);
        }
    }

    ///// <summary>
    ///// Provides a enumerable of <see cref="PageTokenObject"/> based on parameters discribing how many items to get.
    ///// Access the PageTokens through parameter <see cref="PageTokens"/>
    ///// </summary>
    ///// <remarks>
    ///// note the actual number of items returned
    ///// may be anything less than <see cref="RequestRange2.NumberOfItems"/>, or upto <see cref="RequestRange2.MaxResultsPerPage"/> minus 1, more
    ///// </remarks>
    //public class PageTokenRange : RequestRange2
    //{
    //    public readonly IEnumerable<PageTokenObject> PageTokens;
    //    private readonly PageTokenGenerator _pageTokenGenerator = new PageTokenGenerator();

    //    /// <summary>
    //    /// Creates default range with startIndex as 0 and MaxResultsPerPage as 50
    //    /// </summary>
    //    /// <param name="numberOfItems">the total number of items, <remarks>should be a multiple of MaxResultsPerPage, note you are not guranteed this number of items will be returned.</remarks></param>
    //    public PageTokenRange(int numberOfItems)
    //        : this(0, numberOfItems)
    //    {
    //    }

    //    /// <summary>
    //    /// Creates custom range
    //    /// </summary>
    //    /// <param name="startIndex">Index of first item</param>
    //    /// <param name="numberOfItems">the total number of items following startIndex, <remarks>should be a multiple of MaxResultsPerPage, note you are not guranteed this number of items will be returned.</remarks></param>
    //    /// <param name="maxResultsPerPage">integer between 0 and 50 inclusive</param>
    //    public PageTokenRange(int startIndex, int numberOfItems, int maxResultsPerPage = 50)
    //        : base(startIndex, numberOfItems, maxResultsPerPage)
    //    {
    //        this.PageTokens = PageTokensEnumerable();
    //    }

    //    private IEnumerable<PageTokenObject> PageTokensEnumerable()
    //    {
    //        for (var i = 0; i < (Math.Ceiling((double) NumberOfItems/MaxResultsPerPage)); i++)
    //        {
    //            yield return _pageTokenGenerator.GetTokenObject((i*MaxResultsPerPage) + StartIndex);
    //        }
    //    }
    //}

    //public class RequestRange2
    //{
    //    public readonly int MaxResultsPerPage;
    //    public readonly int NumberOfItems;
    //    public readonly int StartIndex;

    //    public RequestRange2(int numberOfItems) : this(0, numberOfItems)
    //    {
    //    }

    //    protected RequestRange2(int startIndex, int numberOfItems, int maxResultsPerPage = 50)
    //    {
    //        if (startIndex < 0)
    //            throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "startIndex must be >= 0");
    //        if (numberOfItems < 1)
    //            throw new ArgumentOutOfRangeException(nameof(numberOfItems), numberOfItems, "numberOfItems must be > 0");
    //        if (maxResultsPerPage < 0 || maxResultsPerPage > 50)
    //            throw new ArgumentOutOfRangeException(nameof(maxResultsPerPage), maxResultsPerPage,
    //                "MaxResultsPerPage must be between 0 and 50 inclusive");

    //        StartIndex = startIndex;
    //        NumberOfItems = numberOfItems;
    //        MaxResultsPerPage = maxResultsPerPage;
    //    }
    //}
}