namespace SignalRGame.Server.Hubs;

public class GameHub : Hub<IGameClient>, IGameHub
{
	private static readonly List<GameRoom> _rooms = new();
    private static readonly List<Player> _players = new();

	public override async Task OnConnectedAsync()
	{
        Console.WriteLine($"Player with id '{Context.ConnectionId}' connected");

        await Clients.Caller.RecieveRoomsList(_rooms);
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var player = _players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
		if (player != null)
		{
            var room = _rooms.Find(r => r.Game.Players.Contains(player));
            if (room is not null)
            {
               await LeaveRoom(room.Id, player.Id);
            }
		}
		Console.WriteLine($"Player with id '{Context.ConnectionId}' disconnected");
	}

	public async Task<GameRoom> CreateRoom(string name, string playerName)
    {
        var room = new GameRoom
        {
            Name = name,
        };

        var player = new Player
        {
            ConnectionId = Context.ConnectionId,
            Name = playerName
        };

        if (!room.TryAddPlayer(player))
        {
			await Clients.Caller.ReceiveError("Error creating room: Room is full or invalid.");
        }

        _rooms.Add(room);

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);
        await Clients.All.RecieveRoomsList(_rooms.OrderBy(r => r.Name).ToList());

        return room;
    }

    public async Task<GameRoom> JoinRoom(string roomId, string playerName)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        if (room is null)
        {
			await Clients.Caller.ReceiveError("Room does not exist");
            return null;
		}

        var player = new Player
        {
            Name = playerName,
            ConnectionId = Context.ConnectionId
        };

        if (!room.TryAddPlayer(player))
        {
			await Clients.Caller.ReceiveError("Error adding player: Room is full.");
			return null;
		}

		_players.Add(player);
		await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);
        await Clients.All.PlayerJoined(roomId, player);

        return room;
    }

    public async Task LeaveRoom(string roomId, string playerId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        if (room is null)
        {
			await Clients.Caller.ReceiveError("Room does not exist.");
			return;
		}

        var player = _players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null)
        {
			await Clients.Caller.ReceiveError("Player not found.");
			return;
		}
        room.Game.Players.Remove(player);
        _players.Remove(player);
        await Clients.Group(roomId).PlayerLeft(roomId, player);

        if (!room.Game.Players.Any())
        {
            _rooms.Remove(room);
            await Clients.All.RecieveRoomsList(_rooms);
        }
    }

    public async Task AddPlayer(string name, string roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        if (room is null)
        {
            await Clients.All.ReceiveError("Room does not exist");
            return;
        }

		var player = new Player
		{
			Name = name,
			ConnectionId = Context.ConnectionId
		};

		_players.Add(player);
		await Clients.All.PlayerJoined(roomId, player);
	}

    public async Task StartGame(string roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        if (room is null)
        {
			await Clients.Caller.ReceiveError("Cannot find room.");
			return;
		}

        room.Game.StartGame();
        await Clients.Group(roomId).GameStarted(roomId, new GameState()
        {
            RoomId = roomId,
            IsGameStarted = true
        });
    }

    public async Task MakeMove(string roomId, string playerId, List<int> diceSelection)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        if (room is not null && room.Game.MakeMove(diceSelection, playerId))
        {
            if (room.Game.RollsRemaining == 0)
                room.Game.NextTurn();

            var winner = room.Game.DetermineRoundWinner();
            // TODO or REMOVE
            //await Clients.Group(roomId).UpdateGameState(room.Game.CurrentGameState);
            await NotifyNextTurn(room.Game.CurrentPlayerId);
        }
        else
        {
            await Clients.All.ReceiveError("Invalid move or room not found");
            return;
        }
    }

    public async Task EndTurn(string roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        if (room is null)
        {
            await Clients.All.ReceiveError("Room does not exist");
            return;
        }

		room.Game.NextTurn();
		await Clients.Group(roomId).NotifyNextTurn(room.Game.CurrentPlayerId);
	}

    public async Task DiceRolled(string playerId, List<int> values)
    {
        var player = _players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null)
        {
            await Clients.All.ReceiveError("Player does not exist");
            return;
        }

        player.SelectDiceToKeep(values);
        player.RollDice();
		await Clients.Caller.DiceRolled(playerId, values);
		await NotifyNextTurn(playerId);
	}

    public async Task DiceFreezed(string playerId, List<int> indeciesToKeep)
    {
		var room = _rooms.FirstOrDefault(r => r.Game.GameState.Players.Any(p => p.Id.Equals(playerId)));
        if (room is not null && room.Game.GameState.IsGameStarted)
        {
            //room.Game. = indicesToKeep;
            await Clients.Group(room.Id).DiceFreezed(playerId, indeciesToKeep);
        }
        else
        {
            await Clients.All.ReceiveError("Player does not exist");
            return;
        }
	}

	public async Task RollDice(string playerId)
	{
		var room = _rooms.FirstOrDefault(r => r.Game.GameState.Players.Any(p => p.Id.Equals(playerId)));
		if (room is not null && room.Game.GameState.IsGameStarted)
		{
            room.Game.RollDice(); // for player id or for currentplayer?
			await Clients.Group(room.Id).DiceRolled(playerId, room.Game.GameState.DiceValues.ToList());
		}
	}

	public async Task FreezeDice(string playerId, List<int> indecesToKeep)
	{
        var player = _players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null)
        {
            return;
        }

        player.SelectDiceToKeep(indecesToKeep);
        await Clients.All.DiceFreezed(playerId, indecesToKeep);
	}

	public async Task NotifyNextTurn(string playerId)
	{
		var player = _players.FirstOrDefault(p => p.ConnectionId.Equals(playerId));
		if (player != null)
		{
			await Clients.All.NotifyNextTurn(playerId);
		}
	}

	public async Task EndGame(string roomId, string winnedId, string score)
	{
		var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
		if (room is null)
		{
            await Clients.All.ReceiveError("Room does not exist");
            return;
		}

		var player = _players.FirstOrDefault(p => p.Id.Equals(winnedId));
        if (player is null)
        {
			await Clients.All.ReceiveError("Player does not exist");
			return;
        }

        decimal.TryParse(score, out decimal prize);

		await Clients.Group(roomId).GameEnded(roomId, new GameResult
        {
            WinnerName = player.Name,
            Prize = prize
        });
	}

	public async Task UpdateRoomsList(List<GameRoom> rooms)
	{
		await Clients.All.RecieveRoomsList(rooms);
	}

	public async Task UpdateGameState(string roomId, GameState gameState)
	{
        await Clients.Group(roomId).RecieveGameState(roomId, gameState);
	}
}
