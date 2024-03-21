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
    private readonly ILogger _logger = Log.Create<DataSeedingModule>();
    private readonly DataSeedingLevel _level;
    private readonly Assembly[] _assemblies;

    public DataSeedingModule(DataSeedingLevel level, params Assembly[] assemblies)
    {
        _level = level;
        _assemblies = assemblies;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        compositionRoot.Register(ServiceDescriptor.Singleton<IDataSeeding>(new DataSeeding(_level)));
        
        var dataSeeders = _assemblies.GetImplementingTypes(typeof(IDataSeeder)).ToArray();

        if (dataSeeders.Any())
        {
            var serviceDescriptors = dataSeeders
                .Select(t => new ServiceDescriptor(typeof(IDataSeeder), t, ServiceLifetime.Scoped));

            compositionRoot.RegisterCollection(serviceDescriptors);
        }
        else
        {
            _logger.LogWarning("No data seeders found");
        }
    }
}
