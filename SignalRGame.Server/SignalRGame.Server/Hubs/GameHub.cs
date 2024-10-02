namespace SignalRGame.Server.Hubs;

public class GameHub : Hub<IGameClient>, IGameHub
{
	private static readonly List<GameRoom> _rooms = new();
    private static readonly List<Player> _players = new();

	// создание комнаты
	public async Task<GameRoom> CreateRoom(string name, string playerName)
	{
		Console.WriteLine("Server_CreateRoom");
		var room = new GameRoom
		{
			Name = name,
		};

		var player = new Player
		{
			ConnectionId = Context.ConnectionId,
			Id = Context.ConnectionId,
			Name = playerName,
			Balance = 500
		};

		Console.WriteLine($"Server_CreateRoom: PlayerId: {player.Id}");

		if (!room.TryAddPlayer(player))
		{
			Console.WriteLine("Server_InvalidAddPlayer");
			await Clients.Caller.ReceiveError("Error creating room: Room is full or invalid.");
		}

		_rooms.Add(room);

		await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);
		Console.WriteLine("Server_RoomCreatedSuccess");
		await Clients.All.RecieveRoomsList(_rooms.OrderBy(r => r.Name).ToList());

		return room;
	}

	// присоединиться к комнате
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

	// покинуть комнату
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

		room.Game.GameState.Players.Remove(player);
		_players.Remove(player);
		await Clients.Group(roomId).PlayerLeft(roomId, player);

		if (!room.Game.GameState.Players.Any())
		{
			_rooms.Remove(room);
			await Clients.All.RecieveRoomsList(_rooms);
		}
	}



	//¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯
	//************************Бизнес-логика игры****************************
	//______________________________________________________________________

	// начало игры
	public async Task StartGame(string roomId)
	{
		Console.WriteLine("Server_StartGame");
		var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
		if (room is null)
		{
			Console.WriteLine("Server_StartGame_NOT_FOUND_ROOM");
			await Clients.Caller.ReceiveError("Cannot find room.");
			return;
		}

		room.Game.StartGame();
		Console.WriteLine($"Server_StartGame: CurrPlayerId: {room.Game.GameState.CurrentPlayerId}");
		Console.WriteLine("Server_StartGame_success");
		await Clients.Group(roomId).GameStarted(roomId, room.Game.GameState);
		await Clients.Group(room.Id).RecieveGameState(room.Id, room.Game.GameState);

		/*new GameState{
			RoomId = roomId,
			IsGameStarted = true
		};*/
	}

    // игрок бросает кубики
	public async Task RollDice(string roomId, string playerId, List<int> dicesToReroll)
	{
		Console.WriteLine("Server_RollDice");
		var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
		Console.WriteLine($"{room.Name}");
		if (room is not null && room.Game.MakeMove(dicesToReroll))
		{
			Console.WriteLine("Server_MakeMove_success");
			room.Game.NextTurn();
			Console.WriteLine("Server_NextTurn_success");
			var winners = new List<Player>();
			winners = room.Game.DetermineRoundWinners();
			Console.WriteLine("Server_DetermineWinner_success");

			await Clients.Group(room.Id).DiceRolled(playerId, winners, dicesToReroll,
													room.Game.GameState.Players
																	.FirstOrDefault(p => p.Id == playerId)
																	.GetValuesDice());
			await Clients.Group(room.Id).RecieveGameState(room.Id, room.Game.GameState);

			if (room.Game.IsGameEnd())
			{
				var winner = room.Game.GetWinner();
				await Clients.Group(roomId).GameEnded(roomId, winner);
				return;
			}

			if (room.Game.IsEndMiniGame())
			{
				room.Game.Reset();
				
				await Clients.Group(roomId).RecieveWinners(roomId, room.Game.GameState);
				return;
			}

			Console.WriteLine("Server_RollDice_success");
			//await Clients.Group(roomId).RecieveGameState(roomId, room.Game.GameState);
			//await NotifyNextTurn(room.Game.GameState.CurrentPlayerId);
		}
		else
		{
			await Clients.All.ReceiveError("Invalid move or room not found");
			return;
		}
	}

	//¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯
	//************************ПАРА-ПАРА-ПАМ Конец***************************
	//______________________________________________________________________



	public async Task UpdateRoomsList(List<GameRoom> rooms)
	{
		await Clients.All.RecieveRoomsList(rooms);
	}

	public async Task UpdateGameState(string roomId, GameState gameState)
	{
		await Clients.Group(roomId).RecieveGameState(roomId, gameState);
	}

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
            var room = _rooms.Find(r => r.Game.GameState.Players.Contains(player));
            if (room is not null)
            {
               await LeaveRoom(room.Id, player.Id);
            }
		}
		Console.WriteLine($"Player with id '{Context.ConnectionId}' disconnected");
	}

	/*public async Task AddPlayer(string name, string roomId)
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
	}*/

	/*public async Task MakeMove(string roomId, List<int> dicesToReroll)
    {
		var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
		if (room is not null && room.Game.MakeMove(dicesToReroll))
		{
			room.Game.NextTurn();

			if (room.Game.IsEndMiniGame())
			{
				room.Game.Reset();
			}

			var winners = room.Game.DetermineRoundWinners();
			// TODO or REMOVE
			await Clients.Group(roomId).RecieveGameState(roomId, room.Game.GameState);
			await NotifyNextTurn(room.Game.GameState.CurrentPlayerId);
		}
		else
		{
			await Clients.All.ReceiveError("Invalid move or room not found");
			return;
		}
	}*/

	/*public async Task EndTurn(string roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        if (room is null)
        {
            await Clients.All.ReceiveError("Room does not exist");
            return;
        }

		room.Game.NextTurn();
		await Clients.Group(roomId).NotifyNextTurn(room.Game.GameState.CurrentPlayerId);
	}*/

	/*public async Task DiceRolled(string playerId, List<int> dicesToReroll)
    {
        var player = _players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null)
        {
            await Clients.All.ReceiveError("Player does not exist");
            return;
        }

        player.SelectDiceToReroll(dicesToReroll);
        player.RollDice();
		await Clients.Caller.DiceRolled(playerId, dicesToReroll, player.GetValuesDice());
		await NotifyNextTurn(playerId);
	}*/

	/*public async Task DiceFreezed(string playerId, List<int> indeciesToKeep)
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
	}*/

	/*public async Task FreezeDice(string playerId, List<int> indecesToKeep)
	{
        var player = _players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null)
        {
            return;
        }

        player.SelectDiceToKeep(indecesToKeep);
        await Clients.All.DiceFreezed(playerId, indecesToKeep);
	}*/

	/*public async Task NotifyNextTurn(string playerId)
	{
		var player = _players.FirstOrDefault(p => p.ConnectionId.Equals(playerId));
		if (player != null)
		{
			await Clients.All.NotifyNextTurn(playerId);
		}
	}*/

	/*public async Task EndGame(string roomId, string winnedId, string score)
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
	}*/
}
