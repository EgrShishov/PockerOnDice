namespace SignalRGame.Domain.Models;

public class Game
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // уникальный айди игры
    public Dictionary<string, IList<int>> Score { get; set; } // результаты каждого игрока
    public List<Player> Players { get; set; } // список всех игроков
    public int CurrentRound { get; set; } = 0;
    public GameState GameState { get; set; }
    public string CurrentPlayerId { get; set; }
    public List<Dice> Dices = new(5); // инвентарь каждого игрока 6 костей, но за ход выбрасывается только 5
    public int RollsRemaining { get; set; } = 3; // количество бросков. По дефолту игрок имеет 3 броска во время хода
    public int ChangeDicesRemaining { get; set; } = 2; // количество попыток 

    public void StartGame()
    {
        if (Players.Count > 1)
        {
            GameState.IsGameStarted = true;
            CurrentRound = 1;
            CurrentPlayerId = Players[0].Id;
            RollsRemaining = 3;
        }
    }

    public void AddPlayer(Player player)
    {
        if (!GameState.IsGameStarted)
        {
            Players.Add(player);
            Score[player.Id] = new List<int>();
        }
    }

    public void NextTurn()
    {
        int currentPlayerIndex = Players.FindIndex(p => p.Id.Equals(CurrentPlayerId));
        int nextPlayerIndex = (currentPlayerIndex + 1) % Players.Count;
        CurrentPlayerId = Players[nextPlayerIndex].Id;

        RollsRemaining = 3;

        if (nextPlayerIndex == 0)
        {
            CurrentRound++; // раунд завершён, нет ходов
        }
    }

    //Первый бросок: Игрок бросает все пять кубиков.
    //Второй и третий броски: После первого броска игрок может выбрать, какие кубики сохранить, а остальные перебросить.
    //Игрок может выполнить до двух перебросов.
    //После третьего броска (или раньше, если игрок решит остановиться после первого или второго броска), ход завершается, и результат фиксируется.
    public void RollDice(List<int> keepIndicies = null)
    {
        if (RollsRemaining > 0)
        {
            Random rnd = new Random();

            for (int i = 0; i < Dices.Count; i++)
            {
                if (keepIndicies is null || !keepIndicies.Contains(i))
                {
                    Dices[i] = new Dice
                    {
                        Value = rnd.Next(1, 7)
                    };
                }
            }

            RollsRemaining--;
        }
        else
        {
            throw new InvalidOperationException("No rolls remaining for this turn.");
        }
    }

    // подсчёт очков конкретного пользователя
    public void CalculateScore(string playerId)
    {
        int score = 0;

        if (!Score.ContainsKey(playerId))
            throw new Exception("Player not found");

        var counts = Dices.GroupBy(x => x)
                           .Select(group => new
                           {
                               Value = group.Key,
                               Count = group.Count()
                           })
                           .OrderByDescending(g => g.Count)
                           .ToList();


    }

    public void EndGame()
    {
        if (GameState.IsGameStarted)
        {
            GameState.IsGameOver = true;
            GameState.IsGameStarted = false;
        }
    }

    public Player GetCurrentPlayer()
    {
        if (Players.Count > 1)
        {
            return Players.First(p => p.Id.Equals(CurrentPlayerId));
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

        var winnerId = Score.OrderByDescending(s => s.Value.Sum())
            .First().Key;

        return Players.Find(p => p.Id.Equals(winnerId));
    }
}
