using System;
using System.Threading;
using Backend.Fx.Exceptions;
using Backend.Fx.Util;

namespace Backend.Fx.DataSeeding.Feature;

public interface IDataSeedingMutex: IDisposable
{
    bool IsAcquired { get; }

    IDisposable Acquire();
}

public class DataSeedingMutex : IDataSeedingMutex
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public bool IsAcquired { get; private set; }

    public IDisposable Acquire()
    {
        if (_semaphore.Wait(0))
        {
            IsAcquired = true;
            return new DelegateDisposable(() =>
            {
                _semaphore.Release();
                IsAcquired = false;
            });
        }

        throw new ConflictedException("Data seeding is already running. Aborting.")
            .AddError("Data seeding is already running. Aborting.");
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}
