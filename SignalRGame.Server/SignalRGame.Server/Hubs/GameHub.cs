namespace SignalRGame.Server.Hubs;

public class GameHub : Hub<Game>
{
    private static readonly List<GameRoom> _rooms = new();
    private static readonly List<Player> _players = new();

    public IHubContext<GameHub> hubContext { get; private set; }

    public async Task<ErrorOr<GameRoom>> JoinGame(string roomId, string playerName)
    {
        var room = _rooms.Find(r => r.Id.Equals(roomId));
        if (room is null)
        {
            return Errors.Rooms.NotFound;
        }

        _players.Add(new Player
        {
            Name = playerName,
            ConnectionId = Context.ConnectionId
        });
    }    
    
    public async Task<ErrorOr<GameRoom>> CreateRoom(string name, string playerName)
    {
        var room = new GameRoom
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
        };

        if (_rooms.Find(r => r.Name.Equals(name)) is null)
        {
            return Errors.Rooms.NotFound;
        }

        _rooms.Add(room);

        return room;
    }

    public async Task AddPlayer(string name)
    {
        //AssertGameNotStarted();
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

}
