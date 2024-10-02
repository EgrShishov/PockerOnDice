using Microsoft.AspNetCore.SignalR.Client;
using SignalRGame.Domain.Abstractions;
using SignalRGame.Domain.Models;

namespace SignalRGame.Client
{
	public class GameClient : IGameClient
	{
		private readonly HubConnection _hubConnection;
		public Dictionary<string, GameRoom> Rooms { get; set; } = new();
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
			//_hubConnection.On<string, List<int>>(nameof(DiceRolled), DiceRolled);
			//_hubConnection.On<string, List<int>>(nameof(DiceFreezed), DiceFreezed);
			_hubConnection.On<string>(nameof(ReceiveError), ReceiveError);
		}

		// Создание комнаты -> на сервер
		public async Task<GameRoom> CreateRoom(string roomName, string playerName)
		{
			Console.WriteLine("Client_CreateRoom");
			var addedRoom = await _hubConnection.InvokeAsync<GameRoom>(nameof(CreateRoom), roomName, playerName);
			Console.WriteLine($"Client_CreateRoom: {addedRoom.Name} - id: {addedRoom.Id}");
			Rooms.TryAdd(addedRoom.Id, addedRoom);
			OnRoomsUpdated?.Invoke();
			return addedRoom;
		}

		// Все доступные комнаты -> с сервера
		public Task RecieveRoomsList(List<GameRoom> rooms)
		{
			Console.WriteLine("Client_RecieveRoomList");

			foreach (var room in rooms)
			{
				Rooms.TryAdd(room.Id, room);
			}

			OnRoomsUpdated?.Invoke();
			Console.WriteLine($"Client_Recieved rooms: {rooms.Count}");
			return Task.CompletedTask;
		}

		// присоединиться к комнате -> на сервер
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

		// игрок присоединился к комнате -> с сервера
		public Task PlayerJoined(string roomId, Player player)
		{
			var room = Rooms[roomId];
			if (room is not null)
			{
				room.Game.GameState.Players.Add(player);
			}
			Console.WriteLine($"Player {player.Name} joined room: {room.Name}");
			OnRoomsUpdated?.Invoke();
			return Task.CompletedTask;
		}

		// покинуть комнату -> на сервер
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

		// игрок покинул комнату -> с сервера
		public Task PlayerLeft(string roomId, Player player)
		{
			var room = Rooms[roomId];
			if (room is not null)
			{
				if (room.Game.GameState.Players.Contains(player))
					room.Game.GameState.Players.Remove(player);
			}
			OnRoomsUpdated?.Invoke();
			return Task.CompletedTask;
		}



		//¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯
		//************************Бизнес-логика игры****************************
		//______________________________________________________________________

		// Начинается игра -> на сервер
		public async Task StartGame(string roomId)
		{
			Console.WriteLine("Client_StartGame");
			await _hubConnection.InvokeAsync(nameof(StartGame), roomId);
			Console.WriteLine("Client_StartGame_succes");
		}

		// начало игры -> с сервера
		public Task GameStarted(string roomId, GameState gameState)
		{
			Console.WriteLine("Client_GameStarted");
			var room = Rooms[roomId];
			if (room is not null)
			{
				room.Game.GameState = gameState;
				OnGameStateUpdated?.Invoke();
				Console.WriteLine($"Client_GameStarted: CurrPlayerId: {room.Game.GameState.CurrentPlayerId}");
				Console.WriteLine("Client_GameStarted_success");
			}
			return Task.CompletedTask;
		}

		// бросок игрока -> на сервер
		public async Task RollDice(string roomId, string playerId, List<int> diceToReroll)
		{
			Console.WriteLine("Client_RollDice");
			await _hubConnection.SendAsync(nameof(RollDice), roomId, playerId, diceToReroll);
			Console.WriteLine("Client_RollDice_success");
			//Console.WriteLine($"Dice rolled {playerId}");
		}

		// результат броска игрока -> с сервера
		public Task DiceRolled(string playerId, List<Player> winners, List<int> dicesToReroll, List<int> diceValues)
		{
			Console.WriteLine("Client_DiceRolled");
			foreach (var value in diceValues)
				Console.WriteLine(value);
			// winners получают какой то отличительный знак
			return Task.CompletedTask;
		}

		// состояние игры изменилось, ход переходить следующему -> с сервера
		public Task RecieveGameState(string roomId, GameState gameState)
		{
			Console.WriteLine("Client_RecieveGameState");
			if (Rooms.TryGetValue(roomId, out GameRoom room))
			{
				room.Game.GameState = gameState;
				OnGameStateUpdated?.Invoke();
				Console.WriteLine($"Client_RecieveGameState: CurrPlayerId: {room.Game.GameState.CurrentPlayerId}");
				Console.WriteLine("Client_RecieveGameState_success");
				//Console.WriteLine($"Game state changed");
			}
			return Task.CompletedTask;
		}

		// когда мини игра окончена: распределить награду между победителями, начать новую игру -> с сервера
		public Task RecieveWinners(string roomId, GameState gameState)
		{
			// всем игрокам выводится сообщение о проигрыше/победе, распределяется награда
			/*if (Rooms.TryGetValue(roomId, out GameRoom room))
			{
				room.Game.GameState = gameState;
				OnGameStateUpdated?.Invoke();
				Console.WriteLine($"Game state changed");
			}*/
			return Task.CompletedTask;
		}

		// конец игры (все монеты у одного игрока) -> с сервера
		public Task GameEnded(string roomId, Player winner)
		{
			// вывод победителю поздравление
			var room = Rooms[roomId];
			if (room is not null)
			{
				room.Game.GameState.IsGameOver = true;
				OnGameStateUpdated?.Invoke();
			}
			return Task.CompletedTask;
		}

		//¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯
		//************************ПАРА-ПАРА-ПАМ Конец***************************
		//______________________________________________________________________



		public string? GetCurrentPlayerId()
		{
			var connectionId = _hubConnection.ConnectionId;
			return connectionId;
		}

		public Task ReceiveError(string errorMessage)
		{
			Console.WriteLine($"Error occured: {errorMessage}");
			return Task.CompletedTask;
		}

		/*public async Task FreezeDice(string playerId, List<int> diceIndecies)
        {
            await _hubConnection.SendAsync(nameof(FreezeDice), playerId, diceIndecies);
        }*/

		/*public Task DiceFreezed(string playerId, List<int> indeciesToKeep)
		{
			Console.WriteLine($"Frezzed indecies to : '{playerId}'");
			foreach (var value in indeciesToKeep)
				Console.WriteLine(value);
			return Task.CompletedTask;
		}*/

		/*public Task TurnEnded(string playerId, List<int> diceToReroll)
		{
			throw new NotImplementedException();
		}*/

		/*public Task NotifyNextTurn(string playerId)
		{
			throw new NotImplementedException();
		}*/
	}
}
