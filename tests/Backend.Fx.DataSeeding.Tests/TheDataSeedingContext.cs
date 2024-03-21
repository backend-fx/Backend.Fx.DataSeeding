using System;
using System.Collections.Generic;
using System.Linq;
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
        var sut = new DataSeedingContext(
            new DataSeeding(DataSeedingLevel.Development),
            new IDataSeeder[]
            {
                new RootSeeder(invocations),
                new Dep2Seeder(invocations),
                new Dep1SeederA(invocations),
                new Dep1SeederB(invocations)
            });

        var graph = sut.BuildDependencyGraph();
        var sorted = sut.TopologicalSort(graph).ToArray();

        // root seeded must be first
        Assert.Equal(typeof(RootSeeder), sorted[0]);

        // dep1 must be seeded before dep2
        Assert.NotEqual(typeof(Dep1SeederB), sorted[3]);
    }

    [Fact]
    public void DetectsCyclesInGraph()
    {
        var invocations = new List<Type>();
        var sut = new DataSeedingContext(
            new DataSeeding(DataSeedingLevel.Demonstration),
            new IDataSeeder[]
            {
                new CyclicSeeder1(invocations),
                new CyclicSeeder2(invocations),
                new CyclicSeeder3(invocations),
            });

        var exception = Record.Exception(() => sut.BuildDependencyGraph());
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void DoesntSwallowIsolatedSeeder()
    {
        var invocations = new List<Type>();
        var sut = new DataSeedingContext(
            new DataSeeding(DataSeedingLevel.Development),
            new IDataSeeder[]
            {
                new IsolatedSeeder1(invocations),
            });

        var graph = sut.BuildDependencyGraph();
        var sorted = sut.TopologicalSort(graph);
        Assert.Single(sorted);
    }

    [Fact]
    public void DoesntSwallowIsolatedSeeders()
    {
        var invocations = new List<Type>();
        var sut = new DataSeedingContext(
            new DataSeeding(DataSeedingLevel.Development),
            new IDataSeeder[]
            {
                new IsolatedSeeder1(invocations),
                new IsolatedSeeder2(invocations),
            });

        var graph = sut.BuildDependencyGraph();
        var sorted = sut.TopologicalSort(graph);
        Assert.Equal(2, sorted.Count());
    }

    private class IsolatedSeeder1(IList<Type> invocations) : TestSeeder(invocations);

    private class IsolatedSeeder2(IList<Type> invocations) : TestSeeder(invocations);

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