using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace YouTubeDataApiWrapper.Util.GoogleApiExtensions
{
    public static class IClientServiceExtensions
    {
        /// <summary>
        /// Determines wether two services use the same AccessTokens or ApiKeys
        /// </summary>
        /// <param name="thisService"></param>
        /// <param name="otherService"></param>
        public static bool AreCompatible(this IClientService thisService, IClientService otherService)
        {
            if (GetAccessToken(thisService) == GetAccessToken(otherService) && thisService.ApiKey == otherService.ApiKey)
                return true;
            return false;
        }
        private static string GetAccessToken(IClientService service)
        {
            try
            {
                return ((UserCredential)service.HttpClientInitializer).Token.AccessToken;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        private static string GetApiKey(IClientService service)
        {
            return service.ApiKey;
        }
    }


}
