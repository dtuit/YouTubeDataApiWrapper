using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YouTubeDataApiWrapper.Util.GoogleApiExtensions;

namespace YouTubeDataApiWrapper.Test
{
    [TestClass]
    public class TestYoutubeServiceEquality
    {
        [TestMethod]
        public void ComparingYoutubeServiceInstancesForEqualityOrCompatiblity()
        {
            var ytService1 = NewTestYouTubeService("OAUTH-ACCESSTOKEN-1", "RefreshToken");
            var ytService2 = NewTestYouTubeService("OAUTH-ACCESSTOKEN-1", "RefreshToken");
            var ytService3 = NewTestYouTubeServiceNonUserAuth("APIKEY-1");

            Assert.AreNotEqual(ytService1, ytService2);
            Assert.AreEqual(ytService1, ytService1);

            Assert.IsTrue(ytService1.AreCompatible(ytService2));
            Assert.IsFalse(ytService1.AreCompatible(ytService3));
        }

        [TestMethod]
        public void BatchRequestTests()
        {
            var ytService1 = NewTestYouTubeService("OAUTH-ACCESSTOKEN-1", "RefreshToken");
            var batchRequest = new BatchRequest(ytService1);
            var x = batchRequest.GetService();
        }
        
        public YouTubeService NewTestYouTubeService(string accessToken, string refreshToken)
        {
            var access_token = accessToken;
            var refresh_token = refreshToken;

            TokenResponse token = new TokenResponse
            {
                AccessToken = access_token,
                RefreshToken = refresh_token
            };

            var cred = new UserCredential
                (new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer()
                    {
                        ClientSecrets = new ClientSecrets()
                        {
                            ClientId = "//clientId",
                            ClientSecret = "//clientSecret"
                        }
                    }
                    ),
                    "testUser1",
                    token
                );

            return new YouTubeService(new BaseClientService.Initializer()
            {
                ApplicationName = this.GetType().ToString(),
                HttpClientInitializer = cred
            });
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
