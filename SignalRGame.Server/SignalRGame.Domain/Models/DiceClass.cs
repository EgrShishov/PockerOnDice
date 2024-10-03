namespace SignalRGame.Domain.Models;

public class DiceClass
{
    public int Id { get; set; }
    public int Value { get; set; }
    public bool IsReroll { get; set; } // если true, то выбран для следующего переброса
    public bool IsRolling { get; set; }

    public DiceClass(int id)
    {
        Id = id;
        IsReroll = true;
        IsRolling = false;
    }
}
