namespace SignalRGame.Domain.Models;

public class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConnectionId { get; set; }
    public string Name { get; set; }
    public List<Dice> Dices { get; set; } = new(6); // инвентарь каждого игрока 6 костей
    public int Score { get; set; } // результат конкретного игрока

    public async Task RollDice(List<int> diceValues) // бросаем кости
    {
        Random rnd = new Random();

        for (int i = 0; i < Dices.Count; i++)
        {
            if (!diceValues.Contains(i))
            {
                Dices[i] = new Dice
                {
                    Value = rnd.Next(1, 7)
                };
            }
        }
    }

    public async Task SelectDiceToKeep(List<int> diceIndices) // определяет какие кости сохраняем
    {
        foreach (var dice in Dices)
        {
            dice.IsSelected = false; // снимаем выделение со всех кубиков
        }

        foreach (var index in diceIndices)
        {
            if (index >= 0 && index < diceIndices.Count)
            {
                Dices[index].IsKeeped = true;
            }
        }
    }

    public async Task SelectDiceToReroll(List<int> diceIndices) // определяет какие кости перебрасываем
    {
        foreach (var dice in Dices)
        {
            dice.IsSelected = false;
        }

        foreach (var index in diceIndices)
        {
            if (index >=0 && index < diceIndices.Count)
            {
                Dices[index].IsSelected = true;
            }
        }
    }
}
