namespace SignalRGame.Domain.Models;

public class PlayerState
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public List<int> Dice { get; set; }
}
