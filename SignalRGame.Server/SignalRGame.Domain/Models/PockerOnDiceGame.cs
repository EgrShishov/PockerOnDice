namespace SignalRGame.Domain.Models;

public class PockerOnDiceGame
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // уникальный айди игры
    //public Dictionary<string, List<int>> Score { get; set; } = new(); // результаты каждого игрока по костям
    //public List<Player> Players { get; set; } = new();// список всех игроков
    public int CurrentRound { get; set; } = 0;
    public GameState GameState { get; set; } = new();
<<<<<<< HEAD
    //public string CurrentPlayerId { get; set; } = string.Empty;
    public int MaxRound { get; set; } = 2; // количество раундов. В каждом раунде игрок выбирает какие кости бросает
    public bool EndMiniGame = false;

    // начало игры
    public bool StartGame()
=======
    public string CurrentPlayerId { get; set; } = string.Empty;
    public List<DiceClass> Dices = new(5); // за ход выбрасывается только 5
    public int RollsRemaining { get; set; } = 3; // количество бросков. По дефолту игрок имеет 3 броска во время хода

    public void StartGame()
>>>>>>> c28d09060da6a67f8667654cbb1edeb440ac7841
    {
        if (GameState.Players.Count > 1)
        {
            GameState.IsGameStarted = true;
            CurrentRound = 1;
            GameState.CurrentPlayerId = GameState.Players[0].Id;
            MaxRound = 2;
            return true;
        }
        return false;
    }

	//Первый раунд(круг): Игрок бросает все пять кубиков.
	//Второй и последующие раунды(круги): После первого круга игрок может выбрать, какие кубики перебросить.
	//После броска ход завершается, результат фиксируется.
	//Делаем ход
	public bool MakeMove(List<int> dicesSelection)
	{
		//if (playerId != GameState.CurrentPlayerId)
		//    return false;

		var currentPlayer = GameState.Players.FirstOrDefault(p => p.Id.Equals(GameState.CurrentPlayerId));
		if (currentPlayer is null)
			return false;
        Console.WriteLine($"Server_MakeMove: CurrPlayer - id: {currentPlayer.Id}, name: {currentPlayer.Name}");
		currentPlayer.SelectDiceToReroll(CurrentRound == 1 ? null : dicesSelection);
		currentPlayer.RollDice();
        // where should i handle keep and select dices logic?
        Console.WriteLine($"Server_CalculateCombination");
		CalculateCombination(GameState.CurrentPlayerId);
		Console.WriteLine($"Server_CalculateCombination_success");
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
        int nextPlayerIndex = (currentPlayerIndex + 1);
        if (nextPlayerIndex == GameState.Players.Count())
        {
            nextPlayerIndex = 0;
            CurrentRound++; // раунд завершён, нет ходов
            if (CurrentRound > MaxRound)
            {
<<<<<<< HEAD
                EndMiniGame = true;
=======
                if (diceSelection is null || !diceSelection.Contains(i))
                {
                    Dices[i] = new DiceClass
                    {
                        Value = rnd.Next(1, 7)
                    };
                }
>>>>>>> c28d09060da6a67f8667654cbb1edeb440ac7841
            }
		}
        GameState.CurrentPlayerId = GameState.Players[nextPlayerIndex].Id;
    }

	/*public void RollDice(List<int> diceSelection = null)
    {
        Random rnd = new Random();

        for (int i = 0; i < Dices.Count; i++)
        {
            if (diceSelection is null || !diceSelection.Contains(i))
            {
                Dices[i] = new Dice
                {
                    Value = rnd.Next(1, 7)
                };
            }
        }
    }*/

    // подсчёт очков конкретного пользователя
    public Combination CalculateCombination(string playerId)
    {
        var player = GameState.Players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (player is null)
        {
            throw new Exception("Player does not exist");
        }

        // check chance, little street, big street
        List<int> check = new List<int>() { 0, 1, 2, 3, 4, 5 };
        bool combination = true;
        for (int i = 0; i < 5; i++)
        {
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
                player.Combo.CombinationLevel = 6;
				return new Combination(6, -1);                 
			}
            if (check[5] == 0)                                // chance:
			{
				player.Combo.CombinationLevel = 5;            // 1, 2, 3, 4, 6 -> 1
				return new Combination(5, -1);                // 1, 2, 3, 5, 6 -> 2
			}                                                 // 1, 2, 4, 5, 6 -> 3
            return new Combination(1, 5 - check.IndexOf(0));  // 1, 3, 4, 5, 6 -> 4
        }
        

        var counts = player.Dices.GroupBy(x => x.Value)
                           .Select(group => new
                           {
                               Value = group.Key,
                               Count = group.Count()
                           })
                           .OrderByDescending(g => g.Count)
                           .ToList(); // группируем кубики у пользователя, допустим [1,1,3,3,3] -> 1 : 2, 3 : 3
        // order
        if (counts[0].Count == 5)
        {
			// poker
			player.Combo.CombinationLevel = 9;
			player.Combo.Score = counts[0].Value;
			return new Combination(9, counts[0].Value);
        }
        else if (counts[0].Count == 4)
        {
			// kare
			player.Combo.CombinationLevel = 8;
			player.Combo.Score = 10 * counts[0].Value + counts[1].Value;
			return new Combination(8, 10 * counts[0].Value + counts[1].Value);
        }
        else if (counts[0].Count == 3 && counts[1].Count == 2)
        {
			// full house
			player.Combo.CombinationLevel = 7;
			player.Combo.Score = 10 * counts[0].Value + counts[1].Value;
			return new Combination(7, 10 * counts[0].Value + counts[1].Value);
        }
        else if (counts[0].Count == 3)
        {
			// set
			player.Combo.CombinationLevel = 4;
			player.Combo.Score = 100 * counts[0].Value + 10 * Math.Max(counts[1].Value, counts[2].Value) + Math.Min(counts[1].Value, counts[2].Value);
			return new Combination(4, 100 * counts[0].Value + 10 * Math.Max(counts[1].Value, counts[2].Value) + Math.Min(counts[1].Value, counts[2].Value));
        }
        else if (counts[0].Count == 2 && counts[1].Count == 2)
        {
			// two pairs
			player.Combo.CombinationLevel = 3;
			player.Combo.Score = 100 * Math.Max(counts[0].Value, counts[1].Value) + 10 * Math.Min(counts[0].Value, counts[1].Value) + counts[2].Value;
			return new Combination(3, 100 * Math.Max(counts[0].Value, counts[1].Value) + 10 * Math.Min(counts[0].Value, counts[1].Value) + counts[2].Value);
        }
        else
        {
            // one pairs
            var values = new List<int> { counts[0].Value, counts[1].Value, counts[2].Value, counts[3].Value };
            values = values.OrderByDescending(x => x).ToList();

			player.Combo.CombinationLevel = 3;
			player.Combo.Score = 1000 * values[0] + 100 * values[1] + 10 * values[2] + values[3];
			return new Combination(2, 1000 * values[0] + 100 * values[1] + 10 * values[2] + values[3]);
        }
    }

    public List<Player> DetermineRoundWinners()
    {
        Console.WriteLine("Server_DetrmineRoundWinners");

        List<Player> sorted_players = GameState.Players.OrderByDescending(p => p.Combo).ToList();
		Console.WriteLine("Server_DetrmineRoundWinners_sortedEnd");
		List<Player> win_players = GameState.Players.Where(p => p.Combo == sorted_players[0].Combo).ToList();

		Console.WriteLine("Server_DetrmineRoundWinners_succes");
		return win_players;
    }

    public bool IsEndMiniGame()
    {
        return EndMiniGame;
    }

    public void Reset()
    {
		GameState.IsGameStarted = true;
        EndMiniGame = false;
		CurrentRound = 1;
		GameState.CurrentPlayerId = GameState.Players[0].Id;
		MaxRound = 2;
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
