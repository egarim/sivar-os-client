using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SivarOs.Models;
using SivarOs.Services;

namespace SivarOs.Views;

public sealed partial class MainPage : Page
{
    private readonly MatrixClient _client;
    private MatrixRoom? _currentRoom;
    private readonly System.Collections.ObjectModel.ObservableCollection<MatrixRoom> _rooms = new();
    private readonly System.Collections.ObjectModel.ObservableCollection<MatrixMessage> _messages = new();

    public MainPage(MatrixClient client)
    {
        _client = client;
        InitializeComponent();
        TxtUserId.Text = _client.CurrentUser?.UserId ?? "";
        RoomList.ItemsSource = _rooms;
        MessageList.ItemsSource = _messages;
        _ = LoadRoomsAsync();
    }

    private async Task LoadRoomsAsync()
    {
        try
        {
            var rooms = await _client.GetJoinedRoomsAsync();
            _rooms.Clear();
            foreach (var room in rooms)
                _rooms.Add(room);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load rooms: {ex.Message}");
        }
    }

    private async void RoomList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RoomList.SelectedItem is not MatrixRoom room) return;
        _currentRoom = room;
        TxtRoomName.Text = room.Name;
        TxtRoomTopic.Text = room.Topic;
        await LoadMessagesAsync(room.RoomId);
    }

    private async Task LoadMessagesAsync(string roomId)
    {
        ChatLoading.IsActive = true;
        _messages.Clear();

        try
        {
            var msgs = await _client.GetMessagesAsync(roomId);
            foreach (var msg in msgs)
                _messages.Add(msg);

            // Scroll to bottom
            MessagesScroll.ChangeView(null, MessagesScroll.ScrollableHeight, null);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load messages: {ex.Message}");
        }
        finally
        {
            ChatLoading.IsActive = false;
        }
    }

    private async void BtnSend_Click(object sender, RoutedEventArgs e)
        => await SendMessageAsync();

    private async void TxtMessage_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await SendMessageAsync();
    }

    private async Task SendMessageAsync()
    {
        if (_currentRoom == null || string.IsNullOrWhiteSpace(TxtMessage.Text)) return;

        var text = TxtMessage.Text.Trim();
        TxtMessage.Text = "";

        var eventId = await _client.SendMessageAsync(_currentRoom.RoomId, text);
        if (eventId != null)
        {
            _messages.Add(new MatrixMessage
            {
                EventId = eventId,
                SenderId = _client.CurrentUser?.UserId ?? "",
                Body = text,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                IsOwn = true
            });
            MessagesScroll.ChangeView(null, MessagesScroll.ScrollableHeight, null);
        }
    }

    private async void BtnNewRoom_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "New Room",
            Content = new TextBox { PlaceholderText = "Room name", Width = 260 },
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var name = ((TextBox)dialog.Content).Text.Trim();
            if (string.IsNullOrEmpty(name)) return;
            var roomId = await _client.CreateRoomAsync(name);
            await LoadRoomsAsync();
        }
    }

    private void ShowError(string msg)
    {
        // TODO: toast notification
        System.Diagnostics.Debug.WriteLine($"[SivarOs] {msg}");
    }
}
