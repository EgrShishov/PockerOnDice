
namespace SignalRGame.Domain.Models;

public class Player
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConnectionId { get; set; } = string.Empty;
    public string Name { get; set; }
    public List<DiceClass> Dices { get; set; } = new() {new DiceClass(0), new DiceClass(1), new DiceClass(2), new DiceClass(3), new DiceClass(4)}; // инвентарь каждого игрока 5 костей
    // Combination(ранг комбинации, score комбинации этого ранга)
    // результат конкретного игрока в текущем раунде. Сбрасывается каждую новую игру.
	public Combination Combo { get; set; } = new();
    public int Balance { get; set; } = 0;

    public void RollDice(List<DiceClass>? dicesToReroll) // бросаем кости
    {
        Console.WriteLine("Server_Player_RollDice");
        if (dicesToReroll is null)
        {
            Console.WriteLine("Server_Player_RollDice - NULL");
            Dices = new() { new DiceClass(0), new DiceClass(1), new DiceClass(2), new DiceClass(3), new DiceClass(4) };
        }

        Random rnd = new Random();

        foreach (var dice in Dices)
        {
            if (dice.IsReroll)
            {
                dice.Value = rnd.Next(1, 7);
                Console.WriteLine(dice.Value);
                dice.IsReroll = false;
            }
        }
        Console.WriteLine("Server_Player_RollDice_success");
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
