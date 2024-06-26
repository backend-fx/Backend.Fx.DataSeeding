using System.Linq;
using System.Reflection;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.DataSeeding.Feature;

internal class DataSeedingModule : IModule
{
    private static readonly ILogger Logger = Log.Create<DataSeedingModule>();
    private readonly IDataSeedingMutex _mutex;
    private readonly Assembly[] _assemblies;

    public DataSeedingModule(IDataSeedingMutex mutex, params Assembly[] assemblies)
    {
        _mutex = mutex;
        _assemblies = assemblies;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        compositionRoot.Register(ServiceDescriptor.Singleton(_mutex));

        var dataSeeders = _assemblies.GetImplementingTypes(typeof(IDataSeeder)).ToArray();

        if (dataSeeders.Any())
        {
            var serviceDescriptors = dataSeeders
                .Select(t => new ServiceDescriptor(typeof(IDataSeeder), t, ServiceLifetime.Scoped))
                .ToArray();
            compositionRoot.RegisterCollection(serviceDescriptors);
            Logger.LogInformation("{Count} data seeders registered", serviceDescriptors.Length);
        }
        else
        {
            Logger.LogWarning("No data seeders found");
        }
    }
}
