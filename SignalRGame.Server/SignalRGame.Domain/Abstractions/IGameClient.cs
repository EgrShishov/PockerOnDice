namespace SignalRGame.Domain.Abstractions;

public interface IGameClient
{
    Task ReceiveMessage<T>(string user, T message) where T : class;
}
