using Microsoft.AspNetCore.SignalR.Client;
using SignalRGame.Domain.Abstractions;
using SignalRGame.Domain.Models;

namespace SignalRGame.Client
{
	public class GameClient : IGameClient
	{
		private readonly HubConnection _hubConnection;
		public Dictionary<string, GameRoom> Rooms { get; set; } = new();
		//public List<GameRoom> Rooms { get; set;} = new();

		public event Action OnRoomsUpdated;
		public event Action OnGameStateUpdated;

		public GameClient(HubConnection hubConnection)
		{
            _hubConnection = hubConnection;
			_hubConnection.On<List<GameRoom>>(nameof(RecieveRoomsList), RecieveRoomsList);
			_hubConnection.On<string, GameState>(nameof(RecieveGameState), RecieveGameState);
			_hubConnection.On<string, Player>(nameof(PlayerJoined), PlayerJoined);
			_hubConnection.On<string, Player>(nameof(PlayerLeft), PlayerLeft);
			_hubConnection.On<string, GameState>(nameof(GameStarted), GameStarted);
			_hubConnection.On<string, List<int>>(nameof(DiceRolled), DiceRolled);
			_hubConnection.On<string, List<int>>(nameof(DiceFreezed), DiceFreezed);
			_hubConnection.On<string>(nameof(ReceiveError), ReceiveError);
		}

		public string? GetCurrentPlayerId()
		{
			var connectionId = _hubConnection.ConnectionId;
			return null;
		}

        public async Task RollDice(string playerId)
        {
            await _hubConnection.SendAsync(nameof(RollDice), playerId);
			Console.WriteLine($"Dice rolled {playerId}");
        }

        public async Task FreezeDice(string playerId, List<int> diceIndecies)
        {
            await _hubConnection.SendAsync(nameof(FreezeDice), playerId, diceIndecies);
        }

		public async Task<GameRoom> JoinRoom(string roomId, string playerName)
		{
			var room = Rooms[roomId];
			if (room is null)
			{
				return null;
			}
			OnRoomsUpdated?.Invoke();
			Console.WriteLine($"Player joined room: {room.Name}");
			return await _hubConnection.InvokeAsync<GameRoom>(nameof(JoinRoom), roomId, playerName);
		}

		public async Task LeaveRoom(string roomId, string playerId)
		{
			var room = Rooms[roomId];
			if (room is null)
			{
				return;
			}
			OnRoomsUpdated?.Invoke();
			Console.WriteLine($"Player left room: {room.Name}");
			await _hubConnection.InvokeAsync(nameof(LeaveRoom), roomId, playerId);
		}

		public async Task<GameRoom> CreateRoom(string roomName, string playerName)
		{
			var addedRoom = await _hubConnection.InvokeAsync<GameRoom>(nameof(CreateRoom), roomName, playerName);
			Rooms.TryAdd(addedRoom.Id, addedRoom);
			OnRoomsUpdated?.Invoke();
			return addedRoom;
		}

		public Task RecieveGameState(string roomId, GameState gameState)
		{
			if (Rooms.TryGetValue(roomId, out GameRoom room))
			{
				room.Game.GameState = gameState;
				OnGameStateUpdated?.Invoke();
				Console.WriteLine($"Game state changed");
			}
			return Task.CompletedTask;
		}

		public Task RecieveRoomsList(List<GameRoom> rooms)
		{
			foreach (var room in rooms)
			{
				Rooms.TryAdd(room.Id, room);
			}

			OnRoomsUpdated?.Invoke();
			Console.WriteLine($"Recieved rooms: {rooms.Count}");
			return Task.CompletedTask;
		}

		public Task PlayerJoined(string roomId, Player player)
		{
			var room = Rooms[roomId];
			if (room is not null)
			{
				room.Game.Players.Add(player);
			}
			Console.WriteLine($"Player {player.Name} joined room: {room.Name}");
			OnRoomsUpdated?.Invoke();
			return Task.CompletedTask;
		}

		public Task PlayerLeft(string roomId, Player player)
		{
			var room = Rooms[roomId];
			if (room is not null)
			{
				if (room.Game.Players.Contains(player))
					room.Game.Players.Remove(player);
			}
			OnRoomsUpdated?.Invoke();
			return Task.CompletedTask;
		}

		public Task DiceRolled(string playerId, List<int> diceValues)
		{
			foreach (var value in diceValues)
				Console.WriteLine(value);
			return Task.CompletedTask;
		}

		public Task DiceFreezed(string playerId, List<int> indeciesToKeep)
		{
			Console.WriteLine($"Frezzed indecies to : '{playerId}'");
			foreach (var value in indeciesToKeep)
				Console.WriteLine(value);
			return Task.CompletedTask;
		}

		public Task TurnEnded(string playerId, List<int> diceToReroll)
		{
			throw new NotImplementedException();
		}

		public Task GameStarted(string roomId, GameState gameState)
		{
			var room = Rooms[roomId];
			if (room is not null)
			{
				room.Game.GameState = gameState;
				OnGameStateUpdated?.Invoke();
			}
			return Task.CompletedTask;
		}

		public Task GameEnded(string roomId, GameResult result)
		{
			var room = Rooms[roomId];
			if (room is not null)
			{
				room.Game.GameState.IsGameOver = true;
				OnGameStateUpdated?.Invoke();
			}
			return Task.CompletedTask;
		}

		public Task ReceiveError(string errorMessage)
		{
			Console.WriteLine($"Error occured: {errorMessage}");
			return Task.CompletedTask;
		}

		public Task NotifyNextTurn(string playerId)
		{
			throw new NotImplementedException();
		}

		public async Task StartGame(string roomId)
		{
			await _hubConnection.InvokeAsync(nameof(StartGame), roomId);
		}
	}
}
