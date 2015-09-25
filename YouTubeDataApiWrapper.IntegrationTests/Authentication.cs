using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;

namespace YouTubeDataApiWrapper.IntegrationTests
{
    public class Authentication
    {
        public static YouTubeService AuthenticateOauthService(string clientId, string clientSecret, string userName)
        {
            string[] scopes = new string[] { YouTubeService.Scope.Youtube};

            try
            {
                UserCredential credential =
                    GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret
                        },
                        scopes,
                        userName,
                        CancellationToken.None,
                        new FileDataStore("dtuit.YouTube.Auth.Store")).Result;

                YouTubeService service = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "YouTube Data Api Wrapper"
                });

                return service;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return null;
            }
        }
        public static YouTubeService AuthenticateOauthService(string apiKey)
        {
            try
            {
                YouTubeService service = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = apiKey,
                    ApplicationName = "YouTube Data API Sample",
                });
                return service;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return null;
            }
        }

        public static YouTubeService AuthenticateOauthService(string clientId, string clientSecret, string userName,
            string accessToken, string refreshToken)
        {
            throw new NotImplementedException();
        }

        //public static async Task<YouTubeService> GetClientYouTubeService()
        //{
        //    //var cred = new UserCredential
        //    //    (new GoogleAuthorizationCodeFlow(
        //    //        new GoogleAuthorizationCodeFlow.Initializer()
        //    //        {
        //    //            ClientSecrets = new ClientSecrets()
        //    //            {
        //    //                ClientSecret = Configuration.ClientSecret,
        //    //                ClientId = Configuration.ClientId
        //    //            }
        //    //        }
        //    //        ),
        //    //        "testUser1",
        //    //        new TokenResponse()
        //    //    );

        //    //await cred.GetAccessTokenForRequestAsync();

        //    var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        //        new ClientSecrets()
        //        {
        //            ClientSecret = Configuration.ClientSecret,
        //            ClientId = Configuration.ClientId
        //        },
        //        // This OAuth 2.0 access scope allows for full read/write access to the
        //        // authenticated user's account.
        //        new[] { YouTubeService.Scope.Youtube },
        //        "user",
        //        CancellationToken.None,
        //        new FileDataStore("")
        //        );

        //    var yts = new YouTubeService(new BaseClientService.Initializer()
        //    {
        //        ApplicationName = "testApp",
        //        HttpClientInitializer = credential
        //    });

        //    return yts;
        //}

        //public static YouTubeService GetServerYouTubeService()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
