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
		Task StartGame(string roomId);
		Task RollDice(string roomId, string playerId, int bet, List<DiceClass> dicesToReroll);
	}
}
