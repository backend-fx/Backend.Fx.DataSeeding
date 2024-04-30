using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Backend.Fx.Execution.Features;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.DataSeeding.Feature;

/// <summary>
/// The feature "Data Seeding" makes sure that all implemented data seeders are executed on application boot
/// </summary>
[PublicAPI]
public class DataSeedingFeature : Execution.Features.Feature, IBootableFeature
{
    private static readonly ILogger Logger = Log.Create<DataSeedingFeature>();

    private readonly DataSeedingLevel _level;

    public DataSeedingFeature(DataSeedingLevel level = DataSeedingLevel.Production)
    {
        _level = level;
    }

    public override void Enable(IBackendFxApplication application)
    {
        Logger.LogInformation("Enabling data seeding for the {ApplicationName}", application.GetType().Name);
        application.CompositionRoot.RegisterModules(new DataSeedingModule(application.Assemblies));
    }

    public virtual async Task BootAsync(
        IBackendFxApplication application,
        CancellationToken cancellationToken = default)
    {
        var context = new DataSeedingContext(application, _level);
        Logger.LogInformation(
            "{ApplicationName} is now seeding data on level {Level}", application.GetType().Name, _level);
        await context.SeedAllAsync(cancellationToken);
    }
}
