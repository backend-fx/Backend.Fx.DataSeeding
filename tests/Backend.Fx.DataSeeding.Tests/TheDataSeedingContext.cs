using System;
using System.Collections.Generic;
using Backend.Fx.DataSeeding.Feature;
using Backend.Fx.DataSeeding.TestApplication;
using Xunit;

namespace Backend.Fx.DataSeeding.Tests;

public class TheDataSeedingContext
{
    [Fact]
    public void SortsSeedersInCorrectOrder()
    {
        var invocations = new List<Type>();
        var sut = new DataSeedingContext(new IDataSeeder[]
        {
            new RootSeeder(invocations),
            new Dep2Seeder(invocations),
            new Dep1SeederA(invocations),
            new Dep1SeederB(invocations)
        });

        var graph = sut.BuildDependencyGraph();
        var sorted = sut.TopologicalSort(graph);

        Assert.Collection(sorted,
            t => Assert.Equal(typeof(RootSeeder), t),
            t => Assert.Equal(typeof(Dep1SeederA), t),
            t => Assert.Equal(typeof(Dep1SeederB), t),
            t => Assert.Equal(typeof(Dep2Seeder), t));
    }

    [Fact]
    public void DetectsCyclesInGraph()
    {
        var invocations = new List<Type>();
        var sut = new DataSeedingContext(new IDataSeeder[]
        {
            new CyclicSeeder1(invocations),
            new CyclicSeeder2(invocations),
            new CyclicSeeder3(invocations),
        });

        var exception = Record.Exception(() => sut.BuildDependencyGraph());
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }


    private class CyclicSeeder1 : TestSeeder
    {
        public CyclicSeeder1(IList<Type> invocations) : base(invocations)
        {
            AddDependency<CyclicSeeder2>();
        }
    }


    private class CyclicSeeder2 : TestSeeder
    {
        public CyclicSeeder2(IList<Type> invocations) : base(invocations)
        {
            AddDependency<CyclicSeeder3>();
        }
    }
    
    private class CyclicSeeder3 : TestSeeder
    {
        public CyclicSeeder3(IList<Type> invocations) : base(invocations)
        {
            AddDependency<CyclicSeeder1>();
        }
    }
}