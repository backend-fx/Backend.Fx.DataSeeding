using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Backend.Fx.Execution.Features;
using Backend.Fx.Execution.Pipeline;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.DataSeeding.Feature;

/// <summary>
/// The feature "Data Seeding" makes sure that all implemented data seeders are executed on application boot
/// </summary>
[PublicAPI]
public class DataSeedingFeature : Execution.Features.Feature, IBootableFeature
{
    public override void Enable(IBackendFxApplication application)
    {
        application.CompositionRoot.RegisterModules(new DataSeedingModule(application.Assemblies));
    }

    public virtual async Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
    {
        await application.Invoker.InvokeAsync(
            async (sp, ct) => await SeedData(sp, ct),
            new SystemIdentity(),
            cancellationToken);
    }

    protected static async Task SeedData(IServiceProvider sp, CancellationToken ct)
    {
        var dataSeeders = sp.GetServices<IDataSeeder>();
        var context = new DataSeedingContext(dataSeeders);
        await context.SeedAllAsync(ct);
    }
}