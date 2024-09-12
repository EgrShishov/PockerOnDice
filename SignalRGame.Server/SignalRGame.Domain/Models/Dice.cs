namespace SignalRGame.Domain.Models;

public class Dice
{
    public int Value { get; set; }
    public bool IsSelected { get; set; } = false; // если true, то выбран для следующего переброса
    public bool IsKeeped { get; set; } = false; // если true, то сохранен
}
