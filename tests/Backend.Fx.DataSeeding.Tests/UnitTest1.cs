using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Backend.Fx.DataSeeding.Feature;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using Xunit;

namespace Backend.Fx.DataSeeding.Tests;

public class UnitTest1 : IDisposable
{
    private readonly TestApplication _app;

    public UnitTest1()
    {
        _app = new TestApplication();
        _app.EnableFeature(new DataSeedingFeature());
    }

    
    [Fact]
    public async Task Test1()
    {
        await _app.BootAsync();
        
        Assert.Equal(4, TestSeeder.Invocations.Count());
    }

    public void Dispose()
    {
        _app.Dispose();
    }
}

public class TestApplication : BackendFxApplication
{
    public TestApplication() : base(
        new SimpleInjectorCompositionRoot(),
        new DebugExceptionLogger(),
        typeof(TestApplication).GetTypeInfo().Assembly)
    {
    }
}

public class RootSeeder : TestSeeder;

public class Dep1SeederA : TestSeeder
{
    public Dep1SeederA()
    {
        AddDependency<RootSeeder>();
    }
}

public class Dep1SeederB : TestSeeder
{
    public Dep1SeederB()
    {
        AddDependency<RootSeeder>();
    }
}

public class Dep2Seeder : TestSeeder
{
    public Dep2Seeder()
    {
        AddDependency<Dep1SeederB>();
    }
}