using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Backend.Fx.Execution.Features;
using JetBrains.Annotations;

namespace Backend.Fx.DataSeeding.Feature;

/// <summary>
/// The feature "Data Seeding" makes sure that all implemented data seeders are executed on application boot
/// </summary>
[PublicAPI]
public class DataSeedingFeature : Execution.Features.Feature, IBootableFeature
{
    private readonly DataSeedingLevel _level;

    public DataSeedingFeature(DataSeedingLevel level = DataSeedingLevel.Production)
    {
        _level = level;
    }

    public override void Enable(IBackendFxApplication application)
    {
        application.CompositionRoot.RegisterModules(new DataSeedingModule(application.Assemblies));
    }

    public virtual async Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
    {
        var context = new DataSeedingContext(application, _level);
        await context.SeedAllAsync(cancellationToken);
    }
}
