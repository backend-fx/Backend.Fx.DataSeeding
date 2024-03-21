using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.DataSeeding.TestApplication;

public abstract class TestSeeder(IList<Type> invocations) : DataSeeder
{
    protected override Task SeedDataAsync(CancellationToken cancellationToken)
    {
        invocations.Add(GetType());
        return Task.CompletedTask;
    }

    protected override Task<bool> ShouldRun(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}