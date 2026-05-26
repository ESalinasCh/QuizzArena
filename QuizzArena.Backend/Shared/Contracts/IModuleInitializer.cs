namespace Shared.Contracts;

public interface IModuleInitializer
{
    void Initialize(IServiceProvider serviceProvider);
}
