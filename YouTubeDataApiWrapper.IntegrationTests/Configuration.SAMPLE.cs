using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeDataApiWrapper.IntegrationTests
{
    class ConfigurationSample //Rename to "Configuration"
    {
        public static readonly string ClientId = "YOUR_CLIENT_ID_HERE";
        public static readonly string ClientSecret = "YOUR_CLIENT_SECRET_HERE";
        public static readonly string RedirectUri = "YOUR_REDIRECT_URI_HERE"; // only with web application

        public static readonly string ApiKey = "YOUR_SERVER_KEY_HERE";

        public static readonly string AccessToken = "OAUTH_ACCESSTOKEN_HERE";
        public static readonly string RefreshToken = "OAUTH_REFRESHTOKEN_HERE";
    }
}
