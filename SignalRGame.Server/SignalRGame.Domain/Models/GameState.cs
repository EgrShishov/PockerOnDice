namespace SignalRGame.Domain.Models;

public class GameState
{
    public string RoomId { get; set; } = string.Empty;
    public List<Player> Players { get; set; } = new();
    public string CurrentPlayerId { get; set; } = string.Empty;
    public bool IsGameStarted { get; set; } = false;
    public bool IsGameOver { get; set; } = false;
}
