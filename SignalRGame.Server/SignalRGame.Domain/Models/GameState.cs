namespace SignalRGame.Domain.Models;

public class GameState
{
    public List<PlayerState> PlayerStates { get; set; }
    public int CurrentPlayerId { get; set; }
    public List<int> DiceValues { get; set; }
    public bool IsGameStarted { get; set; } = false;
    public bool IsGameOver { get; set; } = false;
}
