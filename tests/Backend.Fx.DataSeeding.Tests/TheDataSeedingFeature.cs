using System;
using System.Threading.Tasks;
using Backend.Fx.DataSeeding.Feature;
using Backend.Fx.DataSeeding.TestApplication;
using Xunit;

namespace Backend.Fx.DataSeeding.Tests;

public class TheDataSeedingFeature : IDisposable
{
    private readonly TheApplication _app;

    public TheDataSeedingFeature()
    {
        _app = new TheApplication();
        _app.EnableFeature(new DataSeedingFeature());
    }


    [Fact]
    public async Task CallsAllSeedersOnBoot()
    {
        await _app.BootAsync();
        Assert.Equal(4, _app.GetInvocations().Length);
    }

    public void Dispose()
    {
        _app.Dispose();
    }
}