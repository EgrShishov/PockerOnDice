namespace SignalRGame.Domain.Models;

public class PockerOnDiceGame
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // уникальный айди игры
    public int CurrentRound { get; set; } = 0;
    public GameState GameState { get; set; } = new();
    public int MaxRound { get; set; } = 2; // количество раундов. В каждом раунде игрок выбирает какие кости бросает

    private int _countSteps = 0;

    // начало игры
    public bool StartGame()
    {
        if (GameState.Players.Count > 1)
        {
            GameState.IsGameStarted = true;
            CurrentRound = 1;
            GameState.CurrentPlayerId = GameState.Players[0].Id;
            GameState.GameCurrentBet = 10;
            MaxRound = 2;
            _countSteps = 0;
            return true;
        }
        return false;
    }

	//Первый раунд(круг): Игрок бросает все пять кубиков.
	//Второй и последующие раунды(круги): После первого круга игрок может выбрать, какие кубики перебросить.
	//После броска ход завершается, результат фиксируется.
	//Делаем ход
	public bool MakeMove(int bet, List<DiceClass> dicesToReroll)
	{
		var currentPlayer = GameState.Players.FirstOrDefault(p => p.Id.Equals(GameState.CurrentPlayerId));
		if (currentPlayer is null)
			return false;
        Console.WriteLine($"Server_MakeMove: CurrPlayer - id: {currentPlayer.Id}, name: {currentPlayer.Name}");
		currentPlayer.RollDice(CurrentRound == 1 ? null : dicesToReroll);
        
        if (bet > currentPlayer.Balance + currentPlayer.CurrentBet)
        {
            bet = currentPlayer.Balance + currentPlayer.CurrentBet;
        }
        GameState.TotalPot += (bet - currentPlayer.CurrentBet);
        currentPlayer.Balance -= (bet - currentPlayer.CurrentBet);
        currentPlayer.CurrentBet = bet;
        if (bet > GameState.GameCurrentBet)
        {
            GameState.GameCurrentBet = bet;
        }        

        Console.WriteLine($"Server_CalculateCombination");
		CalculateCombination(GameState.CurrentPlayerId);
		Console.WriteLine($"Server_CalculateCombination_success");
		return true;
	}

    public bool PlayerPass(string PlayerId)
    {
        var currentPlayer = GameState.Players.FirstOrDefault(p => p.Id.Equals(GameState.CurrentPlayerId));
        if (currentPlayer is null)
            return false;
        Console.WriteLine($"Server_PlayerPass: CurrPlayer - id: {currentPlayer.Id}, name: {currentPlayer.Name}");
        currentPlayer.Pass();
        CurrentRound = MaxRound + 1;
        _countSteps = -1;
        return true;
    }

    public void AddPlayer(Player player)
    {
        if (!GameState.IsGameStarted)
        {
            GameState.Players.Add(player);
        }
    }

    public void NextTurn()
    {
        int currentPlayerIndex = GameState.Players.FindIndex(p => p.Id.Equals(GameState.CurrentPlayerId));
        int nextPlayerIndex = (currentPlayerIndex + 1) % GameState.Players.Count();
        GameState.CurrentPlayerId = GameState.Players[nextPlayerIndex].Id;
        
        ++_countSteps;
        if (_countSteps == GameState.Players.Count())
        {
            CurrentRound++; // раунд завершён, нет ходов
            _countSteps = 0;
		}
        if (CurrentRound > MaxRound)
        {
            GameState.EndMiniGame = true;
            GameState.CurrentPlayerId = GameState.Players[currentPlayerIndex].Id;
        }
    }

    // подсчёт очков конкретного пользователя
    public Combination CalculateCombination(string playerId)
    {
        var player = GameState.Players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null)
        {
            throw new Exception("Player does not exist");
        }

        // check chance, little street, big street
        List<int> check = new List<int>() { 0, 0, 0, 0, 0, 0 };
        bool combination = true;
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine(player.Dices[i].Value);
            if (check[player.Dices[i].Value - 1] == 1)
            {
                combination = false;
                break;
            }
            else
                ++check[player.Dices[i].Value - 1];
        }

        if (combination)
        {
            if (check[0] == 0)                            
            {
                Console.WriteLine("BigStreet");
                player.Combo.CombinationLevel = 6;
				return new Combination(6);                 
			}
            if (check[5] == 0)                                // chance:
			{
                Console.WriteLine("SmallStreet");
                player.Combo.CombinationLevel = 5;            // 1, 2, 3, 4, 6 -> 1
				return new Combination(5);                    // 1, 2, 3, 5, 6 -> 2
			}
            Console.WriteLine("Chance");                      // 1, 2, 4, 5, 6 -> 3
            player.Combo.CombinationLevel = 1;
            player.Combo.Score = 5 - check.IndexOf(0);
            return new Combination(1, 5 - check.IndexOf(0));  // 1, 3, 4, 5, 6 -> 4
        }
        

        var counts = player.Dices.GroupBy(x => x.Value)
                           .Select(group => new
                           {
                               Value = group.Key,
                               Count = group.Count()
                           })
                           .OrderByDescending(g => g.Count)
                           .ToList(); // группируем кубики у пользователя, допустим [1,1,3,3,3] -> 3 : 3, 1 : 2
        // order
        if (counts[0].Count == 5)
        {
            // poker
            Console.WriteLine("Poker");
            player.Combo.CombinationLevel = 9;
			player.Combo.Score = counts[0].Value;
			return new Combination(9, counts[0].Value);
        }
        else if (counts[0].Count == 4)
        {
            // kare
            Console.WriteLine("Kare");
            player.Combo.CombinationLevel = 8;
			player.Combo.Score = 10 * counts[0].Value + counts[1].Value;
			return new Combination(8, 10 * counts[0].Value + counts[1].Value);
        }
        else if (counts[0].Count == 3 && counts[1].Count == 2)
        {
            // full house
            Console.WriteLine("FullHouse");
            player.Combo.CombinationLevel = 7;
			player.Combo.Score = 10 * counts[0].Value + counts[1].Value;
			return new Combination(7, 10 * counts[0].Value + counts[1].Value);
        }
        else if (counts[0].Count == 3)
        {
            // set
            Console.WriteLine("Set");
            player.Combo.CombinationLevel = 4;
			player.Combo.Score = 100 * counts[0].Value + 10 * Math.Max(counts[1].Value, counts[2].Value) + Math.Min(counts[1].Value, counts[2].Value);
			return new Combination(4, 100 * counts[0].Value + 10 * Math.Max(counts[1].Value, counts[2].Value) + Math.Min(counts[1].Value, counts[2].Value));
        }
        else if (counts[0].Count == 2 && counts[1].Count == 2)
        {
            // two pairs
            Console.WriteLine("TwoPairs");
            player.Combo.CombinationLevel = 3;
			player.Combo.Score = 100 * Math.Max(counts[0].Value, counts[1].Value) + 10 * Math.Min(counts[0].Value, counts[1].Value) + counts[2].Value;
			return new Combination(3, 100 * Math.Max(counts[0].Value, counts[1].Value) + 10 * Math.Min(counts[0].Value, counts[1].Value) + counts[2].Value);
        }
        else
        {
            // one pairs
            Console.WriteLine("Pair");
            var values = new List<int> { counts[1].Value, counts[2].Value, counts[3].Value };
            values = values.OrderByDescending(x => x).ToList();

			player.Combo.CombinationLevel = 2;
			player.Combo.Score = 1000 * counts[0].Value + 100 * values[0] + 10 * values[1] + values[2];
			return new Combination(2, 1000 * counts[0].Value + 100 * values[0] + 10 * values[1] + values[2]);
        }
    }

    public List<Player> DetermineRoundWinners()
    {
        Console.WriteLine("Server_DetrmineRoundWinners");
        foreach (var player in GameState.Players)
        {
            Console.WriteLine($"Player {player.Name} - {player.Combo.CombinationLevel}, {player.Combo.Score}");
        }
        List<Player> sorted_players = GameState.Players.OrderByDescending(p => p.Combo).ToList();
		Console.WriteLine("Server_DetrmineRoundWinners_sortedEnd");
		List<Player> win_players = GameState.Players.Where(p => p.Combo == sorted_players[0].Combo).ToList();

        if (IsEndMiniGame())
        {
            Console.WriteLine("Server_MiniGameEnd_ TotalRewards");
            int prize = GameState.TotalPot / win_players.Count;
            win_players.ForEach(p => p.Balance += prize);
        }

		Console.WriteLine("Server_DetrmineRoundWinners_succes");
		return win_players;
    }

    public bool IsEndMiniGame()
    {
        return GameState.EndMiniGame;
    }

    public void Reset()
    {
		GameState.IsGameStarted = true;
        GameState.EndMiniGame = false;
		CurrentRound = 1;
		//GameState.CurrentPlayerId = GameState.Players[0].Id;
        GameState.GameCurrentBet = 10;
        GameState.TotalPot = 0;
        MaxRound = 2;
        _countSteps = 0;
        foreach (var player in GameState.Players)
        {
            player.ResetDice();
        }
    }

    private void EndGame()
    {
        if (GameState.IsGameStarted)
        {
            GameState.IsGameOver = true;
            GameState.IsGameStarted = false;
        }
    }

	public bool IsGameEnd()
	{
        return GameState.IsGameOver;
	}

	public Player GetCurrentPlayer()
    {
        if (GameState.Players.Count > 1)
        {
            return GameState.Players.First(p => p.Id.Equals(GameState.CurrentPlayerId));
        }
        else
        {
            throw new Exception("There are no current user.");
        }
    }

    public Player GetWinner()
    {
        if (!GameState.IsGameOver)
        {
            throw new InvalidOperationException("Game is not over yet.");
        }

        var winners = DetermineRoundWinners();

        return winners[0];
    }
}
