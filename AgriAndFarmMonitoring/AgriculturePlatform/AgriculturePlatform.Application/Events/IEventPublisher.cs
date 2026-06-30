// AgriculturePlatform.Application/Events/IEventPublisher.cs
namespace AgriculturePlatform.Application.Events;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : class;
}