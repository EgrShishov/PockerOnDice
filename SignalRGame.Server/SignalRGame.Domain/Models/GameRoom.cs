namespace SignalRGame.Domain.Models;

public class GameRoom
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public PockerOnDiceGame Game { get; set; } = new();
    public List<Player> Players { get; set; } = new();

    public bool TryAddPlayer(Player player)
    {
        if (Players.Count < 2 && !Players.Any(p => p.ConnectionId == player.ConnectionId))
        {
            Players.Add(player);
            return true;
        }

        return false;
    }
}
