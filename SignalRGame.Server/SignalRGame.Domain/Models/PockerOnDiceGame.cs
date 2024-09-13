using System.ComponentModel.Design;
using System.Runtime.CompilerServices;

namespace SignalRGame.Domain.Models;

public class PockerOnDiceGame
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // уникальный айди игры
    public Dictionary<string, List<int>> Score { get; set; } // результаты каждого игрока по костям
    public List<Player> Players { get; set; } // список всех игроков
    public int CurrentRound { get; set; } = 0;
    public GameState GameState { get; set; }
    public string CurrentPlayerId { get; set; }

    public List<Dice> Dices = new(5); // за ход выбрасывается только 5
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
    public void RollDice(List<int> diceSelection = null)
    {
        if (RollsRemaining > 0)
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

            RollsRemaining--;
        }
        else
        {
            throw new InvalidOperationException("No rolls remaining for this turn.");
        }
    }

    // делаем ход
    public bool MakeMove(List<int> dicesSelection, string playerId)
    {
        if (playerId != CurrentPlayerId)
            return false;

        var currentPlayer = Players.FirstOrDefault(p => p.Id.Equals(playerId));
        if (currentPlayer is null)
            return false;

        currentPlayer.RollDice(dicesSelection);
        // where should i handle keep and select dices logic?

        return true;
    }

    // подсчёт очков конкретного пользователя
    public Tuple<int, int> CalculateScore(string playerId)
    {
        int score = 0;

        var player = Players.Find(p => p.Id.Equals(playerId));
        if (player is null)
        {
            throw new Exception("Player does not exist");
        }

        if (!Score.ContainsKey(playerId))
            throw new Exception("Player not found");

        // check chance, little street, big street
        List<int> check = new List<int>(6);
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
            if (check[0] == 0)                             // chance:
                return Tuple.Create(6, 0);                 // 1, 2, 3, 4, 6 -> 1
            if (check[5] == 0)                             // 1, 2, 3, 5, 6 -> 2
                return Tuple.Create(5, 0);                 // 1, 2, 4, 5, 6 -> 3
            return Tuple.Create(1, 5 - check.IndexOf(0));  // 1, 3, 4, 5, 6 -> 4
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
            return Tuple.Create(9, counts[0].Value);
        }
        else if (counts[0].Count == 4)
        {
            // kare
            return Tuple.Create(8, 10 * counts[0].Value + counts[1].Value);
        }
        else if (counts[0].Count == 3 && counts[1].Count == 2)
        {
            // full house
            return Tuple.Create(7, 10 * counts[0].Value + counts[1].Value);
        }
        else if (counts[0].Count == 3)
        {
            // set
            return Tuple.Create(4, 100 * counts[0].Value + 10 * Math.Max(counts[1].Value, counts[2].Value) + Math.Min(counts[1].Value, counts[2].Value));
        }
        else if (counts[0].Count == 2 && counts[1].Count == 2)
        {
            // two pairs
            return Tuple.Create(3, 100 * Math.Max(counts[0].Value, counts[1].Value) + 10 * Math.Min(counts[0].Value, counts[1].Value) + counts[2].Value);
        }
        else
        {
            // one pairs
            var values = new List<int> { counts[0].Value, counts[1].Value, counts[2].Value, counts[3].Value };
            values = values.OrderByDescending(x => x).ToList();

            return Tuple.Create(2, 1000 * values[0] + 100 * values[1] + 10 * values[2] + values[3]);
        }
    }

    public List<Player> DetermineRoundWinner()
    {
        var playerScores = Players.Select(p => new
        {
            Player = p,
            Score = CalculateScore(p.Id),
        }).ToList();

        var sortedPlayers = playerScores.OrderByDescending(p => p.Score.Item1).ToList();

        var max_rang = sortedPlayers[0].Score.Item1;

        sortedPlayers = sortedPlayers.Where(p => p.Score.Item1 == max_rang).GroupBy(x => x.Score.Item2).OrderByDescending(p => p.Key).First().ToList();

        List<Player> win_players = sortedPlayers.Select(p => p.Player).ToList();

        return win_players;
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
