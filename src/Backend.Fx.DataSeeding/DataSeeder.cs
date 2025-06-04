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

    public virtual DataSeedingLevel Level { get; } = DataSeedingLevel.Demonstration;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await ShouldRun(cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation("{DataSeederTypeName} is now seeding data", GetType().FullName);
            await SeedDataAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("{DataSeederTypeName} completed data seeding", GetType().FullName);
        }
        else
        {
            _logger.LogInformation("No need to run {DataSeederTypeName}", GetType().FullName);
        }
    }

    protected void AddDependency<TDataSeeder>() where TDataSeeder : IDataSeeder
    {
        _dependsOn.Add(typeof(TDataSeeder));
    }
    
    protected void AddDependency(string dataSeederType)
    {
        if (string.IsNullOrWhiteSpace(dataSeederType))
        {
            throw new ArgumentException("Data seeder type cannot be null or whitespace.", nameof(dataSeederType));
        }

        var type = Type.GetType(dataSeederType, false);
        if (type == null)
        {
            throw new InvalidOperationException($"{GetType().Name} depends on {dataSeederType} but this type could not be found");
        }

        if (!typeof(IDataSeeder).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"{GetType().Name} depends on {dataSeederType} but this type is not an IDataSeeder");
        }

        _dependsOn.Add(type);
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
