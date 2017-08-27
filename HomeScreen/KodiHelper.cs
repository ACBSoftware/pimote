using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace HomeScreen
{
    [DataContract]
    public class KodiResponse
    {
        [DataMember] public int id { get; set; }
        [DataMember] public string jsonrpc { get; set; }
    }
    public class KodiFileResponse : KodiResponse
    {
        [DataMember] public FileResult result { get; set; }
    }
    public class FileResult
    {
        [DataMember] public KodiFileEntry[] files { get; set; }
        [DataMember] public Limits limits { get; set; }
    }
    public class Limits
    {
        [DataMember] public int end { get; set; }
        [DataMember] public int start { get; set; }
        [DataMember] public int total { get; set; }
    }
    public class KodiFileEntry
    {
        [DataMember] public string file { get; set; }
        [DataMember] public string filetype { get; set; }
        [DataMember] public string label { get; set; }
        [DataMember] public string type { get; set; }
        [DataMember] public string title { get; set; } //doesn't work!?!?
        [DataMember] public string track { get; set; }
        [DataMember(EmitDefaultValue = false, IsRequired = false)] public string[] artist { get; set; } //doesn't work!?!?
    }
    [DataContract]
    public class KodiSortParms
    {
        [DataMember] public string method { get; set; }
        [DataMember] public string order { get; set; }
    }
    [DataContract]
    public class KodiRequest
    {
        [DataMember] public int id { get; set; }
        [DataMember] public string jsonrpc { get; set; }
        [DataMember] public string method { get; set; }
    }
    public class KodiDirectoryParams
    {
        [DataMember] public string directory { get; set; }
        [DataMember] public string media { get; set; }
        [DataMember] public string[] properties { get; set; }
        [DataMember] public KodiSortParms sort { get; set; }
    }

    [DataContract]
    public class KodiDirectoryRequest : KodiRequest
    {
        [DataMember(Name = "params")] public KodiDirectoryParams _params { get; set; }

        public KodiDirectoryRequest() : this("files")
        {
        }
        public KodiDirectoryRequest(String MediaType)
        {
            this.jsonrpc = "2.0";
            this.method = "Files.GetDirectory";
            this._params = new KodiDirectoryParams() { media = "files" };
            this._params.properties = new String[] { "title", "artist", "track", "album", "file", "mimetype"};
            this._params.sort = new KodiSortParms() { method = "none", order = "ascending" };
        }
    }

    [DataContract]
    public class KodiItemRequest
    {
        [DataMember] public String file { get; set; }
    }
    [DataContract]
    public class KodiPlayParams
    {
        [DataMember] public KodiItemRequest item { get; set; }
    }
    [DataContract]
    public class KodiPlayFileRequest : KodiRequest
    {
        [DataMember(Name = "params")] public KodiPlayParams _params { get; set; }
        public KodiPlayFileRequest(String File)
        {//{ "jsonrpc": "2.0", "method": "Player.Open", "params": { "item": { "file": "smb://dan-svr/movies/Kids/My Little Pony Equestria Girls - Rainbow Rocks [2014 480p BDRip].mkv" } }, "id": 1 }
            this.jsonrpc = "2.0";
            this.method = "Player.Open";
            this._params = new KodiPlayParams() { item = new KodiItemRequest() { file = File } };
        }
    }
    [DataContract]
    public class KodiPlayListParams : KodiPlayParams
    {
        [DataMember] public Int32 playlistid { get; set; }
    }
    [DataContract]
    public class KodiAddFileToPlayList : KodiRequest
    {
        [DataMember(Name = "params")]
        public KodiPlayListParams _params { get; set; }
        public KodiAddFileToPlayList(Int32 PlaylistId, String File)
        {//{ "jsonrpc": "2.0", "method": "Player.Open", "params": { "item": { "file": "smb://dan-svr/movies/Kids/My Little Pony Equestria Girls - Rainbow Rocks [2014 480p BDRip].mkv" } }, "id": 1 }
            this.jsonrpc = "2.0";
            this.method = "Playlist.Add";
            this._params = new KodiPlayListParams() { playlistid = PlaylistId, item = new KodiItemRequest() { file = File } };
        }
    }

    [DataContract]
    public class KodiRemoveItemFromPlaylist : KodiRequest
    {
        [DataMember(Name = "params")]
        public KodiOpenPlaylistItem _params { get; set; }
        public KodiRemoveItemFromPlaylist(Int32 PlaylistId, Int32 PositionId)
        {            this.jsonrpc = "2.0";
            this.method = "Playlist.Remove";
            this._params = new KodiOpenPlaylistItem() { playlistid = PlaylistId, position = PositionId };
        }
    }

    [DataContract]
    public class KodiOpenPlaylistItem
    {
        [DataMember] public Int32 playlistid { get; set; }
        [DataMember] public Int32 position { get; set; }
    }
    [DataContract]
    public class KodiOpenPlaylistParams {[DataMember] public KodiOpenPlaylistItem item { get; set; } }
    [DataContract]
    public class KodiOpenPlayList : KodiRequest
    {
        [DataMember(Name = "params")]
        public KodiOpenPlaylistParams _params { get; set; }
        public KodiOpenPlayList(Int32 PlaylistId, Int32 PositionId)
        {
            this.jsonrpc = "2.0";
            this.method = "Player.Open";
            this._params = new KodiOpenPlaylistParams() { item = new KodiOpenPlaylistItem() { playlistid = PlaylistId, position = PositionId } };
        }
    }

    [DataContract]
    public class KodiPlayListIdParams
    {
        [DataMember] public Int32 playlistid { get; set; }
        [DataMember] public String[] properties { get; set; }
    }

    [DataContract]
    public class KodiPlayClearParams
    {
        [DataMember] public Int32 playlistid { get; set; }
    }

    [DataContract]
    public class KodiClearPlayList : KodiRequest
    {
        [DataMember(Name = "params")]
        public KodiPlayClearParams _params { get; set; }
        public KodiClearPlayList(Int32 PlaylistId)
        {
            this.jsonrpc = "2.0";
            this.method = "Playlist.Clear";
            this._params = new KodiPlayClearParams() { playlistid = PlaylistId };
        }
    }

    [DataContract]
    public class KodiPlayListItemsRequest : KodiRequest
    {
        [DataMember(Name = "params")] public KodiPlayListIdParams _params { get; set; }
        public KodiPlayListItemsRequest(Int32 PlaylistId)
        {
            this.jsonrpc = "2.0";
            this.method = "Playlist.GetItems";
            this._params = new KodiPlayListIdParams() { playlistid = PlaylistId };
            this._params.properties = new String[] { "title", "artist", "track", "album", "file" };
        }
    }
    [DataContract]
    public class KodiPlayListItemsResponse : KodiResponse
    {
        [DataMember] public PlayListItemResult result { get; set; }
        public KodiPlayListItemsResponse()
        {
            this.result = new PlayListItemResult();
        }
    }
    [DataContract]
    public class PlayListItemResult
    {
        [DataMember] public KodiPlayListItem[] items { get; set; }
    }
    [DataContract]
    public class KodiPlayListItem
    {
        [DataMember] public Int32 id { get; set; }
        [DataMember] public string title { get; set; }
        [DataMember] public string[] artist { get; set; }
        [DataMember] public string track { get; set; }
        [DataMember] public string album { get; set; }
        [DataMember] public string file { get; set; }
    }


    [DataContract]
    public class KodiGetPlaylists : KodiRequest
    {
        public KodiGetPlaylists()
        { 
            this.jsonrpc = "2.0";
            this.method = "Playlist.GetPlaylists";
        }
    }



    [DataContract]
    public class KodiFileInfoParams
    {
        [DataMember] public string file { get; set; }
        [DataMember] public string media { get; set; }
        [DataMember] public string[] properties { get; set; }
    }

    [DataContract]
    public class KodiFileInfoRequest : KodiRequest
    {
        [DataMember(Name = "params")] public KodiFileInfoParams _params { get; set; }
        public KodiFileInfoRequest(String File)
        {
            this.jsonrpc = "2.0";
            this.method = "Files.GetFileDetails";
            this._params = new KodiFileInfoParams() { media = "files" };
            this._params.file = File;
            this._params.properties = new String[] { "title", "artist", "track", "duration", "album", "file", "mimetype", "size" };
        }
    }


    [DataContract]
    public class KodiPlayerIdParams
    {
        [DataMember] public Int32 playerid { get; set; }
    }

    [DataContract]
    public class KodiMoveParams : KodiPlayerIdParams
    {
        [DataMember] public String direction { get; set; }
    }


    [DataContract]
    public class KodiMoveRequest : KodiRequest
    {
        [DataMember(Name = "params")]
        public KodiMoveParams _params { get; set; }
        public KodiMoveRequest(Int32 PlayerId, String Direction)
        {
            this.jsonrpc = "2.0";
            this.method = "Player.Move";
            this._params = new KodiMoveParams();
            this._params.playerid = PlayerId;
            this._params.direction = Direction;
        }
    }


    [DataContract]
    public class KodiPlayPauseRequest : KodiRequest
    {
        [DataMember(Name = "params")]
        public KodiPlayerIdParams _params { get; set; }
        public KodiPlayPauseRequest(Int32 PlayerId)
        {
            this.jsonrpc = "2.0";
            this.method = "Player.PlayPause";
            this._params = new KodiPlayerIdParams();
            this._params.playerid = PlayerId;

        }
    }


    [DataContract]
    public class KodiPlayerStop : KodiRequest
    {
        [DataMember(Name = "params")]
        public KodiPlayerIdParams _params { get; set; }
        public KodiPlayerStop(Int32 PlayerId)
        {
            this.jsonrpc = "2.0";
            this.method = "Player.Stop";
            this._params = new KodiPlayerIdParams();
            this._params.playerid = PlayerId;
        }
    }

    [DataContract]
    public class KodiSetShuffle : KodiRequest
    {
        [DataMember(Name = "params")]
        public Object[] _params { get; set; }
        public KodiSetShuffle(Int32 PlayerId)
        {
            this.jsonrpc = "2.0";
            this.method = "Player.SetShuffle";
            this._params = new Object[] { PlayerId, "toggle" };
        }
    }

    
    [DataContract]
    public class KodiSetRepeat : KodiRequest
    {
        [DataMember(Name = "params")]
        public Object[] _params { get; set; }
        public KodiSetRepeat(Int32 PlayerId, String Mode)
        {
            // Mode can be "cycle" "off" "all" "one"
            this.jsonrpc = "2.0";
            this.method = "Player.SetRepeat";
            this._params = new Object[] { PlayerId, Mode };
        }
    }
    
    [DataContract]
    public class KodiGetActivePlayers : KodiRequest
    {
        public KodiGetActivePlayers()
        {
            this.jsonrpc = "2.0";
            this.method = "Player.GetActivePlayers";
        }
    }
    [DataContract]
    public class KodiActivePlayersResponse : KodiResponse
    {
        [DataMember] public ActivePlayerResult[] result { get; set; }
        public KodiActivePlayersResponse()
        {
            this.result = new ActivePlayerResult[0];
        }
    }

    [DataContract]
    public class ActivePlayerResult
    {
        [DataMember] public int playerid { get; set; }
        [DataMember] public string type { get; set; }
    }


    public static class KodiCommand
    {
        public static async Task<KodiFileResponse> KodiDirectoryRequest(String KodiBaseURL, KodiDirectoryRequest Request)
        {
            KodiFileResponse Response = new KodiFileResponse();
            try
            {
                String ACommand = HelperMethods.SerializeObject(Request);
                String ResultJson = await SendKodiCommandAsync(KodiBaseURL, ACommand);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(Response.GetType());
                System.IO.MemoryStream stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(ResultJson));
                Response = (KodiFileResponse)ser.ReadObject(stream);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return Response;
        }

        public static async Task<string> SendKodiCommandAsync(String KodiBaseURL, String Command)
        {
            String Result = "";
            //System.Diagnostics.Debug.WriteLine("KODI COMMAND SENT : " + Command);
            try
            {
                //TODO - need to handle timeout on this one...
                String Request = KodiBaseURL;
                if (!KodiBaseURL.EndsWith("/")) { Request += "/"; }
                Request += "jsonrpc?request=" + Uri.EscapeDataString(Command);
                Result = await WebGetUtils.DownloadAStringAsync(Request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            //System.Diagnostics.Debug.WriteLine("KODI COMMAND RESULT: " + Result);
            return Result;
        }

        public static async Task<string> SendKodiCommandsInOrderAsync(String KodiBaseURL, List<String> Commands)
        {
            String Result = "";
            try
            {

                foreach (String Command in Commands)
                {
                    String Request = KodiBaseURL;
                    if (!KodiBaseURL.EndsWith("/")) { Request += "/"; }
                    Request += "jsonrpc?request=" + Uri.EscapeDataString(Command);
                    Result = await WebGetUtils.DownloadAStringAsync(Request);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            //System.Diagnostics.Debug.WriteLine("KODI MULTIPLE COMMAND LAST RESULT: " + Result);
            return Result;
        }

        public static async Task<KodiFileResponse> GetDirectoryContents(String KodiBaseURL, KodiDirectoryRequest Request)
        {
            KodiFileResponse Response = new KodiFileResponse();
            try
            {
                String JsonRequest = HelperMethods.SerializeObject(Request);
                String JsonResult = await SendKodiCommandAsync(KodiBaseURL, JsonRequest);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(Response.GetType());
                System.IO.MemoryStream stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonResult));
                Response = (KodiFileResponse)ser.ReadObject(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return Response;
        }
        public static async Task<KodiPlayListItemsResponse> GetPlaylistItems(String KodiBaseURL, KodiPlayListItemsRequest Request)
        {
            KodiPlayListItemsResponse Response = new KodiPlayListItemsResponse();
            try
            {
                String JsonRequest = HelperMethods.SerializeObject(Request);
                String JsonResult = await SendKodiCommandAsync(KodiBaseURL, JsonRequest);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(Response.GetType());
                System.IO.MemoryStream stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonResult));
                Response = (KodiPlayListItemsResponse)ser.ReadObject(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return Response;
        }
        public static async Task<KodiActivePlayersResponse> GetActivePlayers(String KodiBaseURL)
        {
            KodiActivePlayersResponse Response = new KodiActivePlayersResponse();
            try
            {
                String JsonRequest = HelperMethods.SerializeObject(new KodiGetActivePlayers());
                String JsonResult = await SendKodiCommandAsync(KodiBaseURL, JsonRequest);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(Response.GetType());
                System.IO.MemoryStream stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonResult));
                Response = (KodiActivePlayersResponse)ser.ReadObject(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return Response;
        }
        public static async Task<String> GetPlaylists(String KodiBaseURL)
        {
            
            try
            {
                String JsonRequest = HelperMethods.SerializeObject(new KodiGetPlaylists());
                String JsonResult = await SendKodiCommandAsync(KodiBaseURL, JsonRequest);
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return "";
        }


    }
}
