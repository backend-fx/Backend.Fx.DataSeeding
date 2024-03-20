using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.DataSeeding.Tests;

public abstract class TestSeeder : DataSeeder
{
    private static readonly List<Type> _invocations = new();

    public static IEnumerable<Type> Invocations => _invocations;

    protected override Task SeedDataAsync(CancellationToken cancellationToken)
    {
        _invocations.Add(GetType());
        return Task.CompletedTask;
    }

    protected override Task<bool> ShouldRun()
    {
        return Task.FromResult(true);
    }
}