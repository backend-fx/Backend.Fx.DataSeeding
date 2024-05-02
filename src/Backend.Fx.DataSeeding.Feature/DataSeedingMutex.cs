using System;
using System.Threading;
using Backend.Fx.Exceptions;
using Backend.Fx.Util;

namespace Backend.Fx.DataSeeding.Feature;

public interface IDataSeedingMutex: IDisposable
{
    IDisposable Acquire();
}

public class DataSeedingMutex : IDataSeedingMutex
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public IDisposable Acquire()
    {
        if (_semaphore.Wait(0))
        {
            return new DelegateDisposable(() => _semaphore.Release());
        }

        throw new ConflictedException("Data seeding is already running. Aborting.")
            .AddError("Data seeding is already running. Aborting.");
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}
