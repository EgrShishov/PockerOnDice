namespace SignalRGame.Domain.Models;

public class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConnectionId { get; set; }
    public string Name { get; set; }
    public List<Dice> Dices { get; set; } = new(5); // инвентарь каждого игрока 5 костей
    public int Score { get; set; } // результат конкретного игрока

    public void RollDice() // бросаем кости
    {
        Random rnd = new Random();

        for (int i = 0; i < Dices.Count; i++)
        {
            if (!Dices[i].IsKeeped)
            {
                Dices[i] = new Dice
                {
                    Value = rnd.Next(1, 7)
                };
            }
        }
    }

    public void SelectDiceToKeep(List<int> diceIndices)
    {
        foreach (var dice in Dices)
        {
            dice.IsSelected = false;
        }

        foreach (var index in diceIndices)
        {
            if (index >= 0 && index < diceIndices.Count)
            {
                Dices[index].IsKeeped = true;
            }
        }
    }

/*    public void SelectDiceToReroll(List<int> diceIndices) // определяет какие кости перебрасываем
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
    }*/
}
