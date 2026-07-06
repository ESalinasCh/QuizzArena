namespace Shared.Contracts;

public interface IModuleInitializer
{
    Task Initialize(IServiceProvider serviceProvider);
}
