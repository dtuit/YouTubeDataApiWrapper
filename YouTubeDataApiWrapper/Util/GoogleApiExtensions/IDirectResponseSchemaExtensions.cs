using System.Collections.Generic;
using Google.Apis.Requests;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeDataApiWrapper.Util.GoogleApiExtensions
{
    public static class IDirectResponseSchemaExtensions
    {
        public static IList<TResponseItem> GetResponseItems<TResponseItem>(this IDirectResponseSchema response)
        {
            return response.GetPropertyValue<IList<TResponseItem>>("Items");
        }

        public static string GetNextPageToken(this IDirectResponseSchema response)
        {
            return response.GetPropertyValue<string>("NextPageToken");
        }

        public static PageInfo GetPageInfo(this IDirectResponseSchema response)
        {
            return response.GetPropertyValue<PageInfo>("PageInfo");
        }
    }
}