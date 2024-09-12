namespace SignalRGame.Server.Abstractions;

public interface IGameClient
{
    Task ReceiveDiceRoll(int[] dice);
    Task UpdateGameState(GameState state);
    Task NotifyGameOver(string winner);
}
