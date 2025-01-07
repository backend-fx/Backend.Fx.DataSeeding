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

    public async Task SeedAllAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
    {
        var mutex = application.CompositionRoot.ServiceProvider.GetRequiredService<IDataSeedingMutex>();
        using (mutex.Acquire())
        {
            using (application.UseSingleUserMode())
            {
                var dependencyGraph = GetDataSeederDependencyGraph(application);

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

    protected async Task InvokeSeeder(
        Type seederType,
        IServiceProvider sp,
        DataSeedingLevel seedingLevel,
        CancellationToken ct)
    {
        var dataSeeders = sp.GetServices<IDataSeeder>().ToArray();
        var dataSeeder = dataSeeders.First(s => s.GetType() == seederType);
        if (dataSeeder.Level >= seedingLevel)
        {
            _logger.LogInformation("Invoking {SeederLevel} seeder {SeederType}", seedingLevel, seederType.Name);
            await dataSeeder.SeedAsync(ct);
        }
        else
        {
            _logger.LogInformation(
                "Skipping {SeederLevel} seeder {SeederType} because it is not active for level {Level}",
                dataSeeder.Level,
                seederType.Name,
                seedingLevel);
        }
    }

    private DataSeederDependencyGraph GetDataSeederDependencyGraph(IBackendFxApplication application)
    {
        // Build a dependency graph based on DependsOn property
        using var scope = application.CompositionRoot.BeginScope();
        var dataSeeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var dependencyGraph = new DataSeederDependencyGraph(dataSeeders);
        return dependencyGraph;
    }
}