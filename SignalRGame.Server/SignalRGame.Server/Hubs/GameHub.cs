namespace SignalRGame.Server.Hubs;

public class GameHub : Hub<IGameClient>
{
    private static readonly List<GameRoom> _rooms = new();
    private static readonly List<Player> _players = new();

    //  public IHubContext<GameHub> hubContext { get; private set; }

    public async Task<GameRoom> JoinRoom(string roomId, string playerName)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        if (room is null)
        {
            throw new Exception("Room does not exist");
        }

        var player = new Player
        {
            Name = playerName,
            ConnectionId = Context.ConnectionId
        };

        _players.Add(player);

        if (!room.TryAddPlayer(player))
        {
            throw new Exception("Error in adding player");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);
        await Clients.All.ReceiveMessage("Rooms", _rooms.OrderBy(r => r.Name));

        return room;
    }    
    
    public async Task<GameRoom> CreateRoom(string name, string playerName)
    {
        var room = new GameRoom
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
        };

        var player = new Player
        {
            ConnectionId = Context.ConnectionId,
            Name = playerName
        };

        if (!room.TryAddPlayer(player))
        { 
            throw new Exception("Error in creating room");
        }

        _rooms.Add(room);

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);
        await Clients.Group(room.Id).ReceiveMessage("PlayerJoined", player);

        return room;
    }

    public async Task AddPlayer(string name)
    {
        _players.Add(new Player
        {
            Name = name,
            ConnectionId = Context.ConnectionId
        });
    }

    public async Task StartGame()
    {

    }

    public async Task MakeMove(List<int> DiceSelection)
    {

    }

    public async Task EndTurn()
    {

    }

    public async Task SendMessage<T>(string user, T message) where T : class
        => await Clients.All.ReceiveMessage(user, message);

    public async Task SendMessageToCaller<T>(string user, T message) where T : class
        => await Clients.Caller.ReceiveMessage(user, message);

    public async Task SendMessageToGroup<T>(string group, string user, T message) where T : class
        => await Clients.Group(group).ReceiveMessage(user, message);
}
