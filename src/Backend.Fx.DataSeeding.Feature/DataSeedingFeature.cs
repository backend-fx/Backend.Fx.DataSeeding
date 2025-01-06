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
    private readonly ILogger _logger = Log.Create<DataSeedingFeature>();
    private readonly IDataSeedingContext _dataSeedingContext;
    private readonly IDataSeedingMutex _mutex;

    public DataSeedingFeature(DataSeedingLevel level = DataSeedingLevel.Production, IDataSeedingMutex? mutex = null)
    {
        _dataSeedingContext = new DefaultDataSeedingContext(level);
        _mutex = mutex ?? new DataSeedingMutex();
    }
    
    public DataSeedingFeature(IDataSeedingContext dataSeedingContext, IDataSeedingMutex? mutex = null)
    {
        _dataSeedingContext = dataSeedingContext;
        _mutex = mutex ?? new DataSeedingMutex();
    }

    public override void Enable(IBackendFxApplication application)
    {
        _logger.LogInformation("Enabling data seeding for the {ApplicationName}", application.GetType().Name);
        application.CompositionRoot.RegisterModules(new DataSeedingModule(_mutex, application.Assemblies));
    }

    public virtual async Task BootAsync(
        IBackendFxApplication application,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "{ApplicationName} is now seeding data on level {Level}", 
            application.GetType().Name, 
            _dataSeedingContext.Level);
        
        await _dataSeedingContext.SeedAllAsync(application, cancellationToken);
    }
}
