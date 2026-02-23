using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SivarOs.Models;

namespace SivarOs.Services;

public class MatrixClient
{
    private readonly HttpClient _http;
    private string _accessToken = "";
    private string _userId = "";
    private string _nextBatch = "";

    public string HomeServerUrl { get; set; } = "http://158.220.114.205:8008";
    public MatrixUser? CurrentUser { get; private set; }

    public MatrixClient()
    {
        _http = new HttpClient();
    }

    // ── Auth ────────────────────────────────────────────────────────────────

    public async Task<MatrixUser?> LoginAsync(string username, string password)
    {
        var url = $"{HomeServerUrl}/_matrix/client/v3/login";
        var payload = new
        {
            type = "m.login.password",
            user = username,
            password = password
        };

        var response = await _http.PostAsJsonAsync(url, payload);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        _accessToken = json.GetProperty("access_token").GetString() ?? "";
        _userId = json.GetProperty("user_id").GetString() ?? "";
        _http.DefaultRequestHeaders.Remove("Authorization");
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

        CurrentUser = new MatrixUser
        {
            UserId = _userId,
            AccessToken = _accessToken,
            DeviceId = json.GetProperty("device_id").GetString() ?? "",
            HomeServer = HomeServerUrl
        };

        return CurrentUser;
    }

    public async Task LogoutAsync()
    {
        await _http.PostAsync($"{HomeServerUrl}/_matrix/client/v3/logout", null);
        _accessToken = "";
        _userId = "";
        CurrentUser = null;
        _http.DefaultRequestHeaders.Remove("Authorization");
    }

    // ── Rooms ────────────────────────────────────────────────────────────────

    public async Task<List<MatrixRoom>> GetJoinedRoomsAsync()
    {
        var rooms = new List<MatrixRoom>();
        var response = await _http.GetFromJsonAsync<JsonElement>(
            $"{HomeServerUrl}/_matrix/client/v3/joined_rooms");

        if (response.TryGetProperty("joined_rooms", out var roomIds))
        {
            foreach (var roomIdEl in roomIds.EnumerateArray())
            {
                var roomId = roomIdEl.GetString() ?? "";
                var room = await GetRoomSummaryAsync(roomId);
                rooms.Add(room);
            }
        }

        return rooms;
    }

    private async Task<MatrixRoom> GetRoomSummaryAsync(string roomId)
    {
        var room = new MatrixRoom { RoomId = roomId, Name = roomId };

        try
        {
            var nameResp = await _http.GetFromJsonAsync<JsonElement>(
                $"{HomeServerUrl}/_matrix/client/v3/rooms/{Uri.EscapeDataString(roomId)}/state/m.room.name/");
            if (nameResp.TryGetProperty("name", out var name))
                room.Name = name.GetString() ?? roomId;
        }
        catch { /* no name state event */ }

        try
        {
            var topicResp = await _http.GetFromJsonAsync<JsonElement>(
                $"{HomeServerUrl}/_matrix/client/v3/rooms/{Uri.EscapeDataString(roomId)}/state/m.room.topic/");
            if (topicResp.TryGetProperty("topic", out var topic))
                room.Topic = topic.GetString() ?? "";
        }
        catch { /* no topic */ }

        return room;
    }

    public async Task<string> CreateRoomAsync(string name)
    {
        var payload = new { name, preset = "private_chat" };
        var response = await _http.PostAsJsonAsync(
            $"{HomeServerUrl}/_matrix/client/v3/createRoom", payload);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("room_id").GetString() ?? "";
    }

    // ── Messages ─────────────────────────────────────────────────────────────

    public async Task<List<MatrixMessage>> GetMessagesAsync(string roomId, int limit = 50)
    {
        var messages = new List<MatrixMessage>();
        var url = $"{HomeServerUrl}/_matrix/client/v3/rooms/{Uri.EscapeDataString(roomId)}/messages?dir=b&limit={limit}";
        var response = await _http.GetFromJsonAsync<JsonElement>(url);

        if (response.TryGetProperty("chunk", out var chunk))
        {
            foreach (var evt in chunk.EnumerateArray())
            {
                if (evt.GetProperty("type").GetString() != "m.room.message") continue;

                var content = evt.GetProperty("content");
                messages.Add(new MatrixMessage
                {
                    EventId = evt.GetProperty("event_id").GetString() ?? "",
                    SenderId = evt.GetProperty("sender").GetString() ?? "",
                    Body = content.TryGetProperty("body", out var body) ? body.GetString() ?? "" : "",
                    MsgType = content.TryGetProperty("msgtype", out var mt) ? mt.GetString() ?? "m.text" : "m.text",
                    Timestamp = evt.GetProperty("origin_server_ts").GetInt64(),
                    IsOwn = evt.GetProperty("sender").GetString() == _userId
                });
            }
            messages.Reverse();
        }

        return messages;
    }

    public async Task<string?> SendMessageAsync(string roomId, string text)
    {
        var txnId = Guid.NewGuid().ToString("N");
        var url = $"{HomeServerUrl}/_matrix/client/v3/rooms/{Uri.EscapeDataString(roomId)}/send/m.room.message/{txnId}";
        var payload = new { msgtype = "m.text", body = text };
        var response = await _http.PutAsJsonAsync(url, payload);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("event_id").GetString();
    }
}
