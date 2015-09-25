using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YouTubeDataApiWrapper.IntegrationTests
{
    [TestClass]
    public class SetUpAuth
    {
        [TestMethod]
        public async Task Get_Users_Access_Token()
        {
            try
            {
                var ytService = Authentication.AuthenticateOauthService(Configuration.ClientId, Configuration.ClientSecret, "testUser");
                var userCred = (UserCredential) ytService.HttpClientInitializer;
                
                Debug.WriteLine("AccessToken: " + userCred.Token.AccessToken);
                Debug.WriteLine("RefreshToken: " + userCred.Token.RefreshToken);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.InnerException);
            }
        }
    }
}
