using SignalRGame.Domain.Models;

namespace SignalRGame.Domain.Abstractions;
public interface IGameClient
{
	Task RecieveRoomsList(List<GameRoom> rooms);
	Task PlayerJoined(string roomId, Player player);
	Task PlayerLeft(string roomId, Player player);
	Task GameStarted(string roomId, GameState gameState);
	Task DiceRolled(string roomId, string playerId, List<DiceClass> Rerolldices);
	Task RecieveWinners(string roomId, GameState gameState);
	Task GameEnded(string roomId, Player winner);
	Task ReceiveError(string errorMessage);
	
	Task RecieveGameState(string roomId, GameState gameState);
	//Task NotifyNextTurn(string playerId);
	//Task DiceFreezed(string playerId, List<int> indeciesToKeep);
	//Task TurnEnded(string playerId, List<int> diceToReroll);
}
