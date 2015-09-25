using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeDataApiWrapper.RequestBuilders
{
    public class SubscriptionsListRequestBuilder : BaseListRequestBuilder<SubscriptionsResource.ListRequest, SubscriptionListResponse>
    {
        public SubscriptionsListRequestBuilder(YouTubeService youTubeService, string part) : base(youTubeService, part)
        {
            Mine = true;
            Order = SubscriptionsResource.ListRequest.OrderEnum.Alphabetical;
        }

        public string ChannelId { get; set; }
        public string ForChannelId { get; set; }
        public string Id { get; set; }
        public bool? Mine { get; set; }
        public bool? MySubscribers { get; set; }
        public string OnBehalfOfContentOwner { get; set; }
        public string OnBehalfOfContentOwnerChannel { get; set; }
        public SubscriptionsResource.ListRequest.OrderEnum Order { get; set; }


        public override SubscriptionsResource.ListRequest CreateRequest()
        {
            var req = new SubscriptionsResource.ListRequest(YouTubeService, Part);
            return MapRequestValues(req);
        }
        //public override SubscriptionsResource.ListRequest CreateRequest() {
        //    var req = new SubscriptionsResource.ListRequest(YouTubeService, Part)
        //    {   
        //        ChannelId = ChannelId,
        //        ForChannelId = ForChannelId,
        //        Id = Id,
        //        MaxResults = MaxResults,
        //        Mine = Mine,
        //        MySubscribers = MySubscribers,
        //        OnBehalfOfContentOwner = OnBehalfOfContentOwner,
        //        OnBehalfOfContentOwnerChannel = OnBehalfOfContentOwnerChannel,
        //        Order = Order,
        //        PageToken = PageToken,
        //        Fields = Fields
        //    };
        //    return req;
        //}

        //public override Task<SubscriptionListResponse> GetRequestTask()
        //{
        //    var req = CreateRequest();
        //    return req.ExecuteAsync();
        //}
    }
}
