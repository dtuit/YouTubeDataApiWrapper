using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeDataApiWrapper.RequestBuilders
{
    public class VideosListRequestBuilder : BaseListRequestBuilder<VideosResource.ListRequest, VideoListResponse>
    {
        public VideosListRequestBuilder(YouTubeService youTubeService, string part) : base(youTubeService, part)
        { }
        public VideosResource.ListRequest.ChartEnum? Chart { get; set; }
        public string Hl { get; set; }
        public string Id { get; set; }
        public string Locale { get; set; }
        public VideosResource.ListRequest.MyRatingEnum? MyRating { get; set; }
        public string OnBehalfOfContentOwner { get; set; }
        public string RegionCode { get; set; }
        public string VideoCategoryId { get; set; }
        //public long? DebugProjectIdOverride { get; set; }

        public override VideosResource.ListRequest CreateRequest()
        {
            var req = new VideosResource.ListRequest(YouTubeService, Part);
            return MapRequestValues(req);
        }
    }
}
