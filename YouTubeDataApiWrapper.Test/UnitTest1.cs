using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YouTubeDataApiWrapper.RequestBuilders;

namespace YouTubeDataApiWrapper.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var reqBuilder = new PlaylistItemsListRequestBuilder(NewTestYouTubeServiceNonUserAuth("asdf"), "snippet");
            var req = reqBuilder.CreateRequest();
        }

        public YouTubeService NewTestYouTubeServiceNonUserAuth(string apiKey)
        {
            return new YouTubeService(new BaseClientService.Initializer()
            {
                ApplicationName = this.GetType().ToString(),
                ApiKey = apiKey
            });
        }
    }
}
