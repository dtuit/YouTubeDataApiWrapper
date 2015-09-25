using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YouTubeDataApiWrapper.RequestBuilders;
using YouTubeDataApiWrapper.RequestServices;
using YouTubeDataApiWrapper.Util;

namespace YouTubeDataApiWrapper.IntegrationTests
{
    [TestClass]
    public class TestYouTubeListRequestService
    {
        [TestMethod]
        public async Task Retrieve_Authenticated_Users_Subscriptions()
        {
            var ytService = Authentication.AuthenticateOauthService(Configuration.ClientId, Configuration.ClientSecret, "testUser");

            var subsRequestBuilder = new SubscriptionsListRequestBuilder(ytService, "snippet")
            {
                Mine = true
            };

            var subsListRequestService =
                new YoutubeListRequestService<SubscriptionsResource.ListRequest, SubscriptionListResponse, Subscription>
                    (subsRequestBuilder);

            var subscriptions = await subsListRequestService.ExecuteConcurrentCheckExpectedAsync(new PageTokenRequestRange(500),
                CancellationToken.None);

        }

        [TestMethod]
        public async Task Retrieve_PlaylistItems()
        {
            var ytService = Authentication.AuthenticateOauthService(Configuration.ApiKey);

            var plistItemsListRequestBuilder = new PlaylistItemsListRequestBuilder(ytService, "snippet")
            {
                PlaylistId = "UUsvaJro-UrvEQS9_TYsdAzQ"
            };

            var plistItemsRequestService = new YoutubeListRequestService<PlaylistItemsResource.ListRequest, PlaylistItemListResponse, PlaylistItem>
                (plistItemsListRequestBuilder);

            var playlistItems = await plistItemsRequestService.ExecuteConcurrentAsync(new PageTokenRequestRange(5000));
        }
    }
}
