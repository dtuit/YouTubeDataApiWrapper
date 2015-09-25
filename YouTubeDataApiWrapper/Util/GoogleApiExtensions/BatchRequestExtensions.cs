using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Apis.Requests;
using Google.Apis.Services;

namespace YouTubeDataApiWrapper.Util.GoogleApiExtensions
{
    public static class BatchRequestExtensions 
    {
        public static void QueueManySingleCallback<TResponse>(this BatchRequest batchRequest, IList<IClientServiceRequest> requests,
            BatchRequest.OnResponse<TResponse> callback) where TResponse : class
        {
            if (batchRequest.Count + requests.Count() > 1000)
                throw new InvalidOperationException("A batch request cannot contain more than 1000 single requests");

            foreach (var clientServiceRequest in requests)
            {
                batchRequest.Queue(clientServiceRequest, callback);
            }
        }

        public static BaseClientService GetService(this BatchRequest thisBatchRequest)
        {
            var field = thisBatchRequest.GetType().GetField("service", BindingFlags.Instance | BindingFlags.NonPublic);
            var service = (BaseClientService) field?.GetValue(thisBatchRequest);
            return service;
        }
         
    }
}
