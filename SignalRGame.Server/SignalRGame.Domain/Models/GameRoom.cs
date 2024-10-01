namespace SignalRGame.Domain.Models;

public class GameRoom
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public PockerOnDiceGame Game { get; set; } = new();
    public bool TryAddPlayer(Player player)
    {
        if (Game.Players.Count < 2 && !Game.Players.Any(p => p.ConnectionId == player.ConnectionId))
        {
            Game.AddPlayer(player);
            return true;
        }

        return false;
    }
}
