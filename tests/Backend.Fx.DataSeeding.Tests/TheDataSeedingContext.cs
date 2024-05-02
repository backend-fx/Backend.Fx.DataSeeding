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
            var sut = new DataSeedingContext(_app, DataSeedingLevel.Development);
            await sut.SeedAllAsync();
        }
    }

    [Fact]
    public async Task SeedingProcessIsMutexed()
    {
        _app.EnableFeature(new DataSeedingFeature(DataSeedingLevel.Development));

        await _app.BootAsync();

        var sut1 = new DataSeedingContext(_app, DataSeedingLevel.Development);
        var sut2 = new DataSeedingContext(_app, DataSeedingLevel.Development);
        var task1 = Task.Run(() => sut1.SeedAllAsync());
        var task2 = Task.Run(() => sut2.SeedAllAsync());

        AggregateException ex = Assert.Throws<AggregateException>(() => Task.WaitAll(task1, task2));
        Assert.IsType<ConflictedException>(ex.InnerException);
    }
}
