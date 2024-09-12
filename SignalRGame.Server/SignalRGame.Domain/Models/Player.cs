namespace SignalRGame.Domain.Models;

public class Player
{
    public string Name { get; set; }
    public int Dice { get; set; }
    public int Score { get; set; }
    public int CurrentHead { get; set; }
}
