using System.Threading.Tasks;
using Backend.Fx.DataSeeding.Feature;
using Backend.Fx.DataSeeding.TestApplication;
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
}
