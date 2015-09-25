using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Http;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util;

//TODO: Add timeout to each request and have exponetial backoff retry, may need be implemented in the request.service
//TODO: Retry for specific server errors;

namespace YouTubeDataApiWrapper.RequestServices
{
    /// <summary>
    /// A Multi Request executes multiple indiviual requests concurrently asyncronously to Google Servers .
    ///     You should add a single request using <see cref="Queue{TResponse}"/>
    ///     and execute all requests using <see cref="ExecuteAsync()"/>.
    /// 
    /// <remarks>
    /// MultiRequest is similar in structure to a <see cref="BatchRequest"/> but executes as individiual Http requests, this allows for faster execution than BatchRequest 
    ///     addtionally there are no restrictions to the number of requests or the services they use
    /// 
    /// For best results 
    ///     - the default max simultaionous connections needs to be raised otherwise it will execute slowly.
    ///     - dont execute too many (more than a couple 1000) requests at a time, as this increases the amount of ram needed
    ///     - user must ensure that your usage of the onResponse callback is thread safe i.e store items in a collection from <see cref="System.Collections.Concurrent"/> otherwise data may be lost.
    ///     - set <see cref="RequiresAuth"/> to true or call <see cref="RefreshAllTokens()"/> before calling <see cref="ExecuteAsync()"/> to ensure no unessasary calls to the auth endpoint. 
    /// 
    /// Addtionally there is no guarantee of the order in which the requests will return, <see cref="OnResponse{TResponse}"/> has parameter index, which indicates the order requests where queued.
    /// </remarks>
    /// 
    /// </summary>  
    public class MultiRequest
    {

        private readonly IList<MultiRequest.InnerRequest> _allRequests = new List<MultiRequest.InnerRequest>();
        public int Count => this._allRequests.Count;
        private readonly HashSet<IClientService> _services = new HashSet<IClientService>();

        /// <summary>
        /// When true <see cref="RefreshAllTokens()"/> is called, during <see cref="ExecuteAsync()"/>
        /// </summary>
        public bool RequiresAuth { get; set; }

        /// <summary>
        /// Queues an individaul request
        /// </summary>
        /// <typeparam name="TResponse">The Response type</typeparam>
        /// <param name="request">The individual request </param>
        /// <param name="callback">A callback which will be called on request completion</param>
        public void Queue<TResponse>(IClientServiceRequest request, MultiRequest.OnResponse<TResponse> callback)
            where TResponse : class
        {
            _services.Add(request.Service);

            var innerRequest = new MultiRequest.InnerRequest<TResponse>
            {
                ClientRequest = request,
                ResponseType = typeof (TResponse),
                OnResponseCallback = callback
            };
            this._allRequests.Add(innerRequest);
        }

        /// <summary>
        /// For all the requests services that require Auth if the token is expired refreshes their TokenResponse Object.
        /// <remarks>
        /// If this is not called prior to execution of the MultiRequest the service may refresh the token on every requests, effectivly doubling the number of HttpRequests made.
        /// Setting <see cref="RequiresAuth"/> to true will cause this method to be called on execution.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAllTokens()
        {
            try
            {
                foreach (
                    var userCred in
                        _services.Select(clientService => (UserCredential) clientService.HttpClientInitializer))
                {
                    if(userCred != null && userCred.Token.IsExpired(SystemClock.Default))
                        await userCred.RefreshTokenAsync(CancellationToken.None);
                }
            }
            catch (InvalidCastException) //A service doesnt have a userCredential so is not a autheticated request.
            {
                //Ignored
            }
            catch (TokenResponseException) //A service is not configured correctly. throw for now.
            {
                throw;
            }
        } 

        public Task ExecuteAsync()
        {
            return this.ExecuteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously executes the multi request.
        /// </summary>
        /// <param name="cancellationToken">A cancelationToken that is passed to all requests</param>
        /// <returns></returns>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (RequiresAuth)
            {
                await RefreshAllTokens();
            }
            if (this.Count >= 1)
            {
                var requestTasks = _allRequests.Select((innerRequest, index) => ExecuteParseCallbackTask(innerRequest, index, cancellationToken));
                await Task.WhenAll(requestTasks).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// A task that encapusulates executeing a single request parseing the result and calling its callback.
        /// </summary>
        /// <param name="innerRequest">the innerRequest</param>
        /// <param name="requestIndex">the index of the request</param>
        /// <param name="cancellationToken">a cancelation token</param>
        /// <returns></returns>
        private async Task ExecuteParseCallbackTask(MultiRequest.InnerRequest innerRequest, int requestIndex, CancellationToken cancellationToken)
        {
            ConfigurableHttpClient httpClient = innerRequest.ClientRequest.Service.HttpClient;

            HttpResponseMessage httpResponseMessage;

            using (HttpRequestMessage request = innerRequest.ClientRequest.CreateRequest(new bool?()))
            {
               httpResponseMessage = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string responseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                object content = innerRequest.ClientRequest.Service.Serializer.Deserialize(responseContent,
                    innerRequest.ResponseType);
                innerRequest.OnResponse(content, (RequestError) null, requestIndex, httpResponseMessage);
            }
            else
            {
                RequestError error =
                    await innerRequest.ClientRequest.Service.DeserializeError(httpResponseMessage).ConfigureAwait(false);
                innerRequest.OnResponse((object) null, error, requestIndex, httpResponseMessage);
            }
        }

        /// <summary>
        /// A concrete type callback for an individual response
        /// User needs to ensure that their usage is thread safe.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="content">The content response or <c>null</c> if the response failed</param>
        /// <param name="error">Error or <c>null</c> if the request succeeded.</param>
        /// <param name="index">The request index</param>
        /// <param name="message">The HTTP individual response</param>
        public delegate void OnResponse<in TResponse>(TResponse content, RequestError error, int index, HttpResponseMessage message) where TResponse : class;

        /// <summary>
        /// Inner request encapsulates the each request.
        /// </summary>
        private class InnerRequest
        {
            public IClientServiceRequest ClientRequest { get; set; }

            public Type ResponseType { get; set; }

            public virtual void OnResponse(object content, RequestError error, int index, HttpResponseMessage message)
            {
                var str = message.Headers.ETag?.Tag;
                var directResponseSchema = content as IDirectResponseSchema;
                if (directResponseSchema == null || directResponseSchema.ETag != null || str == null)
                    return;
                directResponseSchema.ETag = str;
            }
        }

        /// <summary>
        /// A generic individual inner request with a generic response type
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        private class InnerRequest<TResponse> : MultiRequest.InnerRequest where TResponse : class
        {
            /// <summary>
            /// gets or sets a concrete type callback for an individual response
            /// </summary>
            public MultiRequest.OnResponse<TResponse> OnResponseCallback { get; set; }

            public override void OnResponse(object content, RequestError error, int index, HttpResponseMessage message)
            {
                base.OnResponse(content, error, index, message);
                this.OnResponseCallback?.Invoke(content as TResponse, error, index, message);
            }
        }
    }


    

}
