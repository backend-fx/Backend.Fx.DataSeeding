using System;
using System.Threading.Tasks;
using Backend.Fx.DataSeeding.Feature;
using Backend.Fx.DataSeeding.TestApplication;
using Backend.Fx.Exceptions;
using Xunit;

namespace Backend.Fx.DataSeeding.Tests;

public class TheDataSeedingContext
{
    private readonly TheApplication _app = new();

    [Fact]
    public async Task CanSeedAll()
    {
        _app.EnableFeature(new DataSeedingFeature(DataSeedingLevel.Development));

        await _app.BootAsync();

        for (int i = 0; i < 3; i++)
        {
            var sut = new DefaultDataSeedingContext(DataSeedingLevel.Development);
            await sut.SeedAllAsync(_app);
        }
    }

    [Fact]
    public async Task SeedingProcessIsMutexed()
    {
        _app.EnableFeature(new DataSeedingFeature(DataSeedingLevel.Development));

        await _app.BootAsync();

        var sut1 = new DefaultDataSeedingContext(DataSeedingLevel.Development);
        var sut2 = new DefaultDataSeedingContext(DataSeedingLevel.Development);
        var task1 = Task.Run(() => sut1.SeedAllAsync(_app));
        var task2 = Task.Run(() => sut2.SeedAllAsync(_app));

        AggregateException ex = Assert.Throws<AggregateException>(() => Task.WaitAll(task1, task2));
        Assert.IsType<ConflictedException>(ex.InnerException);
    }
}
