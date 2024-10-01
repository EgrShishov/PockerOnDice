using SignalRGame.Domain.Models;

namespace SignalRGame.Domain.Abstractions;
public interface IGameClient
{
	Task RecieveGameState(string roomId, GameState gameState);
	Task RecieveRoomsList(List<GameRoom> rooms);
	Task PlayerJoined(string roomId, Player player);
	Task PlayerLeft(string roomId, Player player);
	Task DiceRolled(string playerId, List<int> diceValues);
	Task DiceFreezed(string playerId, List<int> indeciesToKeep);
	Task TurnEnded(string playerId, List<int> diceToReroll);
	Task GameStarted(string roomId, GameState gameState);
	Task GameEnded(string roomId, GameResult result);
	Task ReceiveError(string errorMessage);
	Task NotifyNextTurn(string playerId);
}
