using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.DataSeeding.Feature;

[PublicAPI]
public abstract class DataSeedingContext : IDataSeedingContext
{
    private readonly ILogger _logger = Log.Create<DataSeedingContext>();

    protected DataSeedingContext(DataSeedingLevel level)
    {
        Level = level;
    }

    public async Task SeedAllAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
    {
        var mutex = application.CompositionRoot.ServiceProvider.GetRequiredService<IDataSeedingMutex>();
        using (mutex.Acquire())
        {
            using (application.UseSingleUserMode())
            {
                var dependencyGraph = GetDataSeederDependencyGraph(application);

                // Execute SeedAsync on each seeder in order
                foreach (var seederType in dependencyGraph.GetSortedSeederTypes())
                {
                    using (_logger.LogInformationDuration(
                               $"Invoking seeder {seederType.Name}",
                               $"Invoking seeder {seederType.Name} done."))
                    {
                        await RunSeederInSeparateInvocationAsync(application, seederType, cancellationToken);
                    }
                }
            }
        }
    }

    protected abstract Task RunSeederInSeparateInvocationAsync(
        IBackendFxApplication application,
        Type seederType,
        CancellationToken cancellationToken);

    public DataSeedingLevel Level { get; }

    private DataSeederDependencyGraph GetDataSeederDependencyGraph(IBackendFxApplication application)
    {
        // Build a dependency graph based on DependsOn property
        using var scope = application.CompositionRoot.BeginScope();
        var dataSeeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var dependencyGraph = new DataSeederDependencyGraph(dataSeeders);
        return dependencyGraph;
    }
}