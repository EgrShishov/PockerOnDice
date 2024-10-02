
namespace SignalRGame.Domain.Models;

public class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConnectionId { get; set; }
    public string Name { get; set; }
    public List<DiceClass> Dices { get; set; } = new List<DiceClass>() { new DiceClass(), new DiceClass(), new DiceClass(), new DiceClass(), new DiceClass()};
	public Combination Combo { get; set; } = new();
    public int Balance { get; set; }
	public decimal CurrentBet { get; set; } = 0;

	public void RollDice() // бросаем кости
    {
        Random rnd = new Random();

        for (int i = 0; i < Dices.Count; i++)
        {
            if (Dices[i].IsReroll)
            {
                Dices[i].IsReroll = false;
                Dices[i] = new DiceClass
                {
                    Value = rnd.Next(1, 7)
                };
            }
        }
    }

    public void SelectDiceToReroll(List<int>? diceIndices)
    {
        Console.WriteLine("SelectDiceToReroll");
        if (diceIndices is null)
        {
			Console.WriteLine("SelectDiceToReroll - NULL");
			diceIndices = new List<int> { 0, 1, 2, 3, 4 };
        }
        foreach (var index in diceIndices)
        {
			Console.WriteLine($"SelectDiceToReroll - index: {index}");
			if (index >= 0 && index < diceIndices.Count)
            {
                Dices[index].IsReroll = true;
            }
        }
		Console.WriteLine("SelectDiceToReroll_success");
	}

	public List<int> GetValuesDice()
	{
        var dices_values = new List<int>();
        foreach (var dice in Dices)
        {
            dices_values.Add(dice.Value);
        }
		return dices_values;
	}
}
