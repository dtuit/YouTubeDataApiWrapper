using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Requests;
using Google.Apis.Services;
using YouTubeDataApiWrapper.Util.GoogleApiExtensions;

namespace YouTubeDataApiWrapper.RequestServices
{

    public class MultiBatchRequest
    {
        public List<BatchRequest> BatchRequests = new List<BatchRequest>();
        private BatchRequest _currentBatchRequest;
        private const int MaxPerBatch = 1000;

        //private List<BaseClientService> ServiceObjects = new List<BaseClientService>();  

        public void Queue<TResponse>(IClientServiceRequest request, BatchRequest.OnResponse<TResponse> callback) where TResponse : class
        {
            SetCurrentBatchRequest(request);

            try
            {
                _currentBatchRequest.Queue(request, callback);
            }
            catch (InvalidOperationException ex)
            {
                throw;
            }
        }

        private void SetCurrentBatchRequest(IClientServiceRequest request)
        {
            // Not Initalized Yet
            if (BatchRequests.Count > 1 || Object.Equals(_currentBatchRequest, default(BatchRequest)))
            {
                InitalizeNewAndSetAsCurrentBatchRequest((BaseClientService) request.Service);
            }
            // Make Sure Compatible
            else if (!_currentBatchRequest.GetService().AreCompatible((BaseClientService) request.Service))
            {
                bool hasChanged = false;
                foreach (var batchRequest in BatchRequests)
                {
                    if (batchRequest.Count < MaxPerBatch &&
                        batchRequest.GetService().AreCompatible((BaseClientService) request.Service))
                    {
                        _currentBatchRequest = batchRequest;
                        hasChanged = true;
                        break;
                        //InitalizeNewAndSetAsCurrentBatchRequest((BaseClientService) request.Service);
                    }
                }
                if (!hasChanged)
                {
                    InitalizeNewAndSetAsCurrentBatchRequest((BaseClientService) request.Service);
                }
            }
            // Within limit
            else if (_currentBatchRequest.Count  > MaxPerBatch)
            {
                InitalizeNewAndSetAsCurrentBatchRequest((BaseClientService) request.Service);
            }

        }

        private void InitalizeNewAndSetAsCurrentBatchRequest(BaseClientService youTubeService)
        {
            var req = new BatchRequest(youTubeService);
            BatchRequests.Add(req);
            SetCurrentRequest(req);
        }

        private void SetCurrentRequest(BatchRequest request)
        {
            _currentBatchRequest = request;
        }

        public async Task ExecuteAllAsync()
        {
            var reqTasks = BatchRequests.Select(req => req.ExecuteAsync()).ToArray();
            await Task.WhenAll(reqTasks);
        }



        
        //public void ConstructBatchRequests(IList<IClientServiceRequest> requests, BatchRequest.OnResponse<TResponse> callback) { }
    }

}
