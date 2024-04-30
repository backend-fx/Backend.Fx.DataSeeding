using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.DataSeeding.Feature;

public class DataSeedingContext
{
    private readonly ILogger _logger = Log.Create<DataSeedingContext>();
    private readonly IBackendFxApplication _application;
    private readonly DataSeedingLevel _dataSeedingLevel;

    public DataSeedingContext(IBackendFxApplication application, DataSeedingLevel dataSeedingLevel)
    {
        _application = application;
        _dataSeedingLevel = dataSeedingLevel;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        var dependencyGraph = GetDataSeederDependencyGraph();

        // Execute SeedAsync on each seeder in order
        foreach (var seederType in dependencyGraph.GetSortedSeederTypes())
        {
            await RunSeederInSeparateInvocationAsync(cancellationToken, seederType);
        }
    }

    private DataSeederDependencyGraph GetDataSeederDependencyGraph()
    {
        // Build a dependency graph based on DependsOn property
        using var scope = _application.CompositionRoot.BeginScope();
        var dataSeeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var dependencyGraph = new DataSeederDependencyGraph(dataSeeders);
        return dependencyGraph;
    }

    private async Task RunSeederInSeparateInvocationAsync(CancellationToken cancellationToken, Type seederType)
    {
        using (_logger.LogInformationDuration(
                   $"Invoking seeder {seederType.Name}",
                   $"Invoking seeder {seederType.Name} done."))
        {
            await _application.Invoker.InvokeAsync(
                async (sp, ct) =>
                {
                    var dataSeeders = sp.GetServices<IDataSeeder>().ToArray();
                    var dataSeeder = dataSeeders.First(s => s.GetType() == seederType);
                    if (dataSeeder.Level >= _dataSeedingLevel)
                    {
                        await dataSeeder.SeedAsync(ct);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Skipping seeder {SeederType} because it is not active for level {Level}",
                            seederType.Name,
                            dataSeeder.Level);
                    }
                },
                new SystemIdentity(),
                cancellationToken,
                allowInvocationDuringBoot: true);
        }
    }
}
