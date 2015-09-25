# YouTube Data API Wrapper
A small C# libary that wraps the [Google.Apis.YouTube](https://developers.google.com/api-client-library/dotnet/apis/youtube/v3 "YouTube Data .Net home page") .NET client.
##### Purpose
- Allow for paginated requests to be executed concurrently (by generating pageTokens).
- Extract large amounts of API data in a short period of time.

#### Installation
nuget : 

For optimal usage the maximum number of connections must be raised by adding the following to the `app.config`
```xml
<configuration>
...
  <system.net>
    <connectionManagement>
      <add address="*" maxconnection="1000" />
    </connectionManagement>
  </system.net>
...</configuration>
```
### Usage

```c#
var ytService = new YouTubeService(/*...Create Authorized YouTubeService*/)

//Get 5000 videos from a uploads playlist
var plistItemsListRequestBuilder = new PlaylistItemsListRequestBuilder(ytService, "snippet")
    {
        PlaylistId = "UUsvaJro-UrvEQS9_TYsdAzQ"
    };
var plistItemsRequestService = 
    new YoutubeListRequestService<PlaylistItemsResource.ListRequest, PlaylistItemListResponse, PlaylistItem>(plistItemsListRequestBuilder);

var playlistItems = await plistItemsRequestService.ExecuteConcurrentAsync(new PageTokenRequestRange(5000));

```

#### Dependencies






