using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;

namespace Backend.Fx.DataSeeding.Feature;

public interface IDataSeedingContext
{
    Task SeedAllAsync(IBackendFxApplication application, CancellationToken cancellationToken = default);
}