using ErrorOr;

namespace SignalRGame.Domain.Errors;

public partial class Errors
{
    public static class Rooms
    {
        public static Error NotFound => Error.NotFound(
            code: "Rooms.DoesNotExist",
            description: "There are no such room");
    }

    public static class Game
    {
        public static Error NotEnoughPlayers => Error.Failure(
            code: "Game.NotEnoughPlayers",
            description: "The game require at least 2 players");
    }
}
