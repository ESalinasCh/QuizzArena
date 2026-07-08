using Shared.Contracts;

namespace Host.Extensions;

public static class MigrationExtension
{
    public static async Task ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var initializers = scope.ServiceProvider.GetServices<IModuleInitializer>();
        foreach (var item in initializers)
        {
            await item.Initialize(app.ApplicationServices);
        }
    }
}
