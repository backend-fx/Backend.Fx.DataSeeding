using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Fx.DataSeeding.Feature;
using Backend.Fx.DataSeeding.TestApplication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Fx.DataSeeding.Tests;

public class TheDataSeedingFeature : IDisposable
{
    private readonly TheApplication _app = new();

    [Fact]
    public async Task CallsAllSeedersOnBootThatMatchTheSeedingModeDemonstration()
    {
        _app.EnableFeature(new DataSeedingFeature(DataSeedingLevel.Demonstration));
        await _app.BootAsync();

        // sanity check
        Assert.NotEmpty(_app.GetInvocations());

        IDataSeeder[] seeders;
        using (var scope = _app.CompositionRoot.BeginScope())
        {
            seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        }

        foreach (var seeder in seeders)
        {
            Assert.Contains(_app.GetInvocations(), t => t == seeder.GetType());
        }
    }

    [Fact]
    public async Task CallsAllSeedersOnBootThatMatchTheSeedingModeDevelopment()
    {
        _app.EnableFeature(new DataSeedingFeature(DataSeedingLevel.Development));
        await _app.BootAsync();

        // sanity check
        Assert.NotEmpty(_app.GetInvocations());

        IDataSeeder[] seeders;
        using (var scope = _app.CompositionRoot.BeginScope())
        {
            seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        }

        var demoSeeders = seeders.Where(s => s.Level == DataSeedingLevel.Demonstration).ToArray();
        var devSeeders = seeders.Where(s => s.Level == DataSeedingLevel.Development).ToArray();
        var prodSeeders = seeders.Where(s => s.Level == DataSeedingLevel.Production).ToArray();

        foreach (var seeder in demoSeeders)
        {
            Assert.DoesNotContain(_app.GetInvocations(), t => t == seeder.GetType());
        }

        foreach (var seeder in devSeeders)
        {
            Assert.Contains(_app.GetInvocations(), t => t == seeder.GetType());
        }

        foreach (var seeder in prodSeeders)
        {
            Assert.Contains(_app.GetInvocations(), t => t == seeder.GetType());
        }
    }

    [Fact]
    public async Task CallsAllSeedersOnBootThatMatchTheSeedingModeProduction()
    {
        _app.EnableFeature(new DataSeedingFeature());
        await _app.BootAsync();

        // sanity check
        Assert.NotEmpty(_app.GetInvocations());

        IDataSeeder[] seeders;
        using (var scope = _app.CompositionRoot.BeginScope())
        {
            seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        }

        var demoSeeders = seeders.Where(s => s.Level == DataSeedingLevel.Demonstration).ToArray();
        var devSeeders = seeders.Where(s => s.Level == DataSeedingLevel.Development).ToArray();
        var prodSeeders = seeders.Where(s => s.Level == DataSeedingLevel.Production).ToArray();

        foreach (var seeder in demoSeeders)
        {
            Assert.DoesNotContain(_app.GetInvocations(), t => t == seeder.GetType());
        }

        foreach (var seeder in devSeeders)
        {
            Assert.DoesNotContain(_app.GetInvocations(), t => t == seeder.GetType());
        }

        foreach (var seeder in prodSeeders)
        {
            Assert.Contains(_app.GetInvocations(), t => t == seeder.GetType());
        }
    }

    public void Dispose()
    {
        _app.Dispose();
    }
}
