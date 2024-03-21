using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.DataSeeding;

[PublicAPI]
public abstract class DataSeeder : IDataSeeder
{
    private readonly ILogger _logger = Log.Create<DataSeeder>();
    private readonly List<Type> _dependsOn = new();

    public IEnumerable<Type> DependsOn => _dependsOn;

    public virtual DataSeedingLevel Level { get; } = DataSeedingLevel.Production;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await ShouldRun(cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation("{DataGeneratorTypeName} is now seeding data", GetType().FullName);
            await SeedDataAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("{DataGeneratorTypeName} completed data seeding", GetType().FullName);
        }
        else
        {
            _logger.LogInformation("No need to run {DataGeneratorTypeName}", GetType().FullName);
        }
    }

    protected void AddDependency<TDataSeeder>() where TDataSeeder : IDataSeeder
    {
        _dependsOn.Add(typeof(TDataSeeder));
    }
    
    /// <summary>
    /// Implement your seeding logic here
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected abstract Task SeedDataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// return true, if the generator should be executed. Seeders must be implemented idempotent, 
    /// since they're all executed on each application start
    /// </summary>
    /// <returns></returns>
    protected abstract Task<bool> ShouldRun(CancellationToken cancellationToken);
}