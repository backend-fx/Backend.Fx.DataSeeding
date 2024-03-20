using System.Linq;
using System.Reflection;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.DataSeeding.Feature;

internal class DataSeedingModule : IModule
{
    private readonly Assembly[] _assemblies;

    public DataSeedingModule(params Assembly[] assemblies)
    {
        _assemblies = assemblies;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        compositionRoot.RegisterCollection(
            _assemblies
                .GetImplementingTypes(typeof(IDataSeeder))
                .Select(t => new ServiceDescriptor(typeof(IDataSeeder), t, ServiceLifetime.Scoped)));
    }
}
