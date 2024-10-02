using System.Runtime.CompilerServices;

namespace SignalRGame.Domain.Models;

public class GameState
{
    public string RoomId { get; set; } = string.Empty;
    //public List<PlayerState> PlayerStates { get; set; } = new();
    public List<Player> Players { get; set; } = new();
    public string CurrentPlayerId { get; set; } = string.Empty;
    //public List<int> DiceValues { get; set; } = new();
    public bool IsGameStarted { get; set; } = false;
    public bool IsGameOver { get; set; } = false;
	public decimal TotalPot { get; set; } = 0;

	public void RaiseBet(string playerId, decimal amount)
	{
		Players.Find(p => p.Id.Equals(playerId)).CurrentBet += amount;
		TotalPot += amount;
	}

	public void Check(string playerId)
	{

	}
	
	public void Pass(string playerId)
	{

	}
}
