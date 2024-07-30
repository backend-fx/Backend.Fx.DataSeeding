using System;
using System.Collections.Generic;
using Backend.Fx.DataSeeding.Feature;
using Backend.Fx.DataSeeding.TestApplication;
using Xunit;

namespace Backend.Fx.DataSeeding.Tests;

public class TheDataSeederDependencyGraph
{
    [Fact]
    public void SortsSeedersInCorrectOrder()
    {
        var invocations = new List<Type>();
        var sut = new DataSeederDependencyGraph(
            new IDataSeeder[]
            {
                new RootSeeder(invocations),
                new Dep2Seeder(invocations),
                new Dep1SeederA(invocations),
                new Dep1SeederB(invocations)
            });

        var sorted = sut.GetSortedSeederTypes();

        // root seeded must be first
        Assert.Equal(typeof(RootSeeder), sorted[0]);

        // dep1 must be seeded before dep2
        Assert.NotEqual(typeof(Dep1SeederB), sorted[3]);
    }

    [Fact]
    public void DetectsCyclesInGraph()
    {
        var invocations = new List<Type>();

        var exception = Record.Exception(() => new DataSeederDependencyGraph(
            new IDataSeeder[]
            {
                new CyclicSeeder1(invocations),
                new CyclicSeeder2(invocations),
                new CyclicSeeder3(invocations),
            }));
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void DoesntSwallowIsolatedSeeder()
    {
        var invocations = new List<Type>();
        var sut = new DataSeederDependencyGraph(
            new IDataSeeder[]
            {
                new IsolatedSeeder1(invocations),
            });

        var sorted = sut.GetSortedSeederTypes();
        Assert.Single(sorted);
    }

    [Fact]
    public void DoesntSwallowIsolatedSeeders()
    {
        var invocations = new List<Type>();
        var sut = new DataSeederDependencyGraph(
            new IDataSeeder[]
            {
                new IsolatedSeeder1(invocations),
                new IsolatedSeeder2(invocations),
            });

        var sorted = sut.GetSortedSeederTypes();
        Assert.Equal(2, sorted.Length);
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
