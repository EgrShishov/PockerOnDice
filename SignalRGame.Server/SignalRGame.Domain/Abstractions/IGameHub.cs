using SignalRGame.Domain.Models;

namespace SignalRGame.Domain.Abstractions
{
	public interface IGameHub
	{
		Task<GameRoom> CreateRoom(string roomName, string playerName);
		Task<GameRoom> JoinRoom(string roomId, string playerName);
		Task LeaveRoom(string roomId, string playerName);
		Task UpdateRoomsList(List<GameRoom> rooms);
		Task UpdateGameState(string roomId, GameState gameState);
		Task RollDice(string playerId);
		Task FreezeDice(string playerId, List<int> indecesToKeep);
		Task NotifyNextTurn(string playerId);
		Task StartGame(string roomId);
		Task EndGame(string roomId, string winnedId, string score);
		Task EndTurn(string roomId);
	}
}
