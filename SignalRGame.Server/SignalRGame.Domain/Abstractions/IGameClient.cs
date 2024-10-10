using SignalRGame.Domain.Models;

namespace SignalRGame.Domain.Abstractions;
public interface IGameClient
{
	Task RecieveRoomsList(List<GameRoom> rooms);
	Task PlayerJoined(string roomId, Player player);
	Task PlayerLeft(string roomId, Player player);
	Task GameStarted(string roomId, GameState gameState);
    Task GameContinued(string roomId, GameState gameState);
	Task MiniGameEnded(string roomId, int reward);
	Task GameEnded(string roomId, Player winner);
	Task ReceiveError(string errorMessage);
	Task RecieveGameState(string roomId, GameState gameState);
	Task RecieveWinners(string roomId, List<Player> winners);
}
