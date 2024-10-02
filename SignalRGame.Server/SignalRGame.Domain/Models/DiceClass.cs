    namespace SignalRGame.Domain.Models;

public class DiceClass
{
    public int Id { get; set; }
    public int Value { get; set; }
    public bool IsReroll { get; set; } = true; // если true, то выбран для следующего переброса
}
