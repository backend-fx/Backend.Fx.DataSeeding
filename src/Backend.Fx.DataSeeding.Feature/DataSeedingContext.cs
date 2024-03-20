namespace Backend.Fx.DataSeeding.Feature;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class DataSeedingContext
{
    private readonly IEnumerable<IDataSeeder> _seeders;

    public DataSeedingContext(IEnumerable<IDataSeeder> seeders)
    {
        _seeders = seeders;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        // Build a dependency graph based on DependsOn property
        Dictionary<Type, HashSet<Type>> dependencyGraph = BuildDependencyGraph();

        // Topologically sort the seeders based on dependencies
        var sortedSeeders = TopologicalSort(dependencyGraph);

        // Execute SeedAsync on each seeder in order
        foreach (var seederType in sortedSeeders)
        {
            var seeder = _seeders.First(s => s.GetType() == seederType);
            await seeder.SeedAsync(cancellationToken);
        }
    }

    private Dictionary<Type, HashSet<Type>> BuildDependencyGraph()
    {
        var dependencyGraph = new Dictionary<Type, HashSet<Type>>();

        foreach (var seeder in _seeders)
        {
            foreach (var dependency in seeder.DependsOn)
            {
                if (!dependencyGraph.ContainsKey(dependency))
                {
                    dependencyGraph[dependency] = new HashSet<Type>();
                }

                dependencyGraph[dependency].Add(seeder.GetType());
            }
        }

        return dependencyGraph;
    }

    private IEnumerable<Type> TopologicalSort(Dictionary<Type, HashSet<Type>> dependencyGraph)
    {
        var visited = new HashSet<Type>();
        var result = new List<Type>();

        foreach (var node in dependencyGraph.Keys)
        {
            Visit(node, dependencyGraph, visited, result);
        }

        return result;
    }

    private void Visit(
        Type node,
        Dictionary<Type, HashSet<Type>> dependencyGraph,
        HashSet<Type> visited,
        List<Type> result)
    {
        if (visited.Add(node))
        {
            if (dependencyGraph.TryGetValue(node, out var value))
            {
                foreach (var dependency in value)
                {
                    Visit(dependency, dependencyGraph, visited, result);
                }
            }

            result.Add(node);
        }
    }
}