using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Exceptions;
using Backend.Fx.Execution.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.DataSeeding.Feature;

public class EnsureDataSeedingIsNotRunningDecorator : IOperation
{
    private readonly IOperation _operation;
    private readonly IDataSeedingMutex _mutex;

    public EnsureDataSeedingIsNotRunningDecorator(IOperation operation, IDataSeedingMutex mutex)
    {
        _operation = operation;
        _mutex = mutex;
    }


    public Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
    {
        if (_mutex.IsAcquired)
        {
            throw new ConflictedException("Data seeding is currently running. Aborting.")
                .AddError("Data seeding is currently running. Aborting.");
        }

        return _operation.BeginAsync(serviceScope, cancellationToken);
    }

    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        return _operation.CompleteAsync(cancellationToken);
    }

    public Task CancelAsync(CancellationToken cancellationToken = default)
    {
        return _operation.CancelAsync(cancellationToken);
    }
}
