
using Shared.Contracts;

public static class MigrationExtension
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var initializers = scope.ServiceProvider.GetServices<IModuleInitializer>();
        foreach (var item in initializers)
        {
            item.Initialize(app.ApplicationServices);
        }
    }
}