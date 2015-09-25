using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeDataApiWrapper.RequestBuilders
{
    public class PlaylistItemsListRequestBuilder : BaseListRequestBuilder<PlaylistItemsResource.ListRequest, PlaylistItemListResponse>
    {
        public PlaylistItemsListRequestBuilder(YouTubeService youTubeService, string part) : base(youTubeService, part) { }

        public string Id { get; set; }
        public string OnBehalfOfContentOwner { get; set; }
        public string PlaylistId { get; set; }
        public string VideoId { get; set; }

        public override PlaylistItemsResource.ListRequest CreateRequest()
        {
            var req = new PlaylistItemsResource.ListRequest(YouTubeService, Part);
            return MapRequestValues(req);
        }
        //public override PlaylistItemsResource.ListRequest CreateRequest()
        //{
        //    var req = new PlaylistItemsResource.ListRequest(YouTubeService, Part)
        //    {
        //        Id = Id,
        //        OnBehalfOfContentOwner = OnBehalfOfContentOwner,
        //        PlaylistId = PlaylistId,
        //        VideoId = VideoId,
        //        MaxResults = MaxResults,
        //        PageToken = PageToken,
        //        Fields = Fields
        //    };
        //    return req;
        //}

        //public override Task<PlaylistItemListResponse> GetRequestTask()
        //{
        //    var req = CreateRequest();
        //    return req.ExecuteAsync();
        //}
    }

}
