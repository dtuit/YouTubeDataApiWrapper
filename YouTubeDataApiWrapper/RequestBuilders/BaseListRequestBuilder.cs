using System.Threading.Tasks;
using Google.Apis.Requests;
using Google.Apis.YouTube.v3;

namespace YouTubeDataApiWrapper.RequestBuilders
{
    //TODO: create annotation to mark propertys that are to be used.
    public abstract class BaseListRequestBuilder<TRequest, TResponse> 
        where TRequest : YouTubeBaseServiceRequest<TResponse>
        where TResponse : IDirectResponseSchema
    {
        /// <summary>
        /// Creates a new instance of TRequest and intializes parameters acording propertys of this
        /// </summary>
        /// <returns></returns>
        public abstract TRequest CreateRequest();

        public virtual Task<TResponse> GetRequestTask()
        {
            var req = this.CreateRequest();
            return req.ExecuteAsync();
        }

        /// Probably a bad idea
        /// <summary>
        /// Creates a new instance of <typeparamref name="TRequest"/> using reflection and instatiates the propertys as per (this) BaseListRequest propertys
        /// </summary>
        /// <returns></returns>
        //public virtual TRequest CreateRequest2()
        //{
        //    var constructors = typeof(TRequest).GetConstructors();
        //    var constructorInfo = typeof(TRequest).GetConstructor(new[] { typeof(IClientService), typeof(string)});
        //    if (constructorInfo == null) return null;   // null not ideal
        //    var request = (TRequest)constructorInfo.Invoke(new object[] {this.YouTubeService, this.Part});
        //    MapRequestValues(request);
        //    return request;
        //}

        /// <summary>
        /// Map the propertys of ListRequestBuilder to TRequest using reflection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual TRequest MapRequestValues(TRequest request)
        {
            foreach (var prop in this.GetType().GetProperties())
            {
                request.GetType().GetProperty(prop.Name)?.SetValue(request, prop.GetValue(this, null));
            }
            return request;
        }

        public string Part { get; set; }
        public string Fields { get; set; }
        public string PageToken { get; set; }
        public long? MaxResults { get; set; } = 50;
                
        public string Key { get; set; }
        public string OauthToken { get; set; }
        public bool? PrettyPrint { get; set; }
        public string QuotaUser { get; set; }
        public string UserIp { get; set; }

        public YouTubeService YouTubeService { get; set; }

        protected BaseListRequestBuilder(YouTubeService youTubeService, string part)
        {
            this.YouTubeService = youTubeService;
            Part = part;
        }
    }
}
