using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.DataSeeding.Feature;

public class DataSeedingContext
{
    private readonly IDataSeeding _dataSeeding;
    private readonly IEnumerable<IDataSeeder> _seeders;

    public DataSeedingContext(IDataSeeding dataSeeding, IEnumerable<IDataSeeder> seeders)
    {
        _dataSeeding = dataSeeding;
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

            if (seeder.Level >= _dataSeeding.Level)
            {
                await seeder.SeedAsync(cancellationToken);
            }
        }
    }

    public Dictionary<Type, HashSet<Type>> BuildDependencyGraph()
    {
        var dependencyGraph = new Dictionary<Type, HashSet<Type>>();

        // Add all seeders to the graph
        foreach (var seeder in _seeders)
        {
            if (!dependencyGraph.ContainsKey(seeder.GetType()))
            {
                dependencyGraph[seeder.GetType()] = new HashSet<Type>();
            }
        }

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

        // Detect cycles in the dependency graph
        if (HasCycle(dependencyGraph))
        {
            throw new InvalidOperationException(
                "Cycle detected in dependency graph. Unable to perform topological sorting.");
        }

        return dependencyGraph;
    }

    public bool HasCycle(Dictionary<Type, HashSet<Type>> dependencyGraph)
    {
        var visited = new HashSet<Type>();
        var path = new HashSet<Type>();

        foreach (var node in dependencyGraph.Keys)
        {
            if (HasCycleUtil(node, dependencyGraph, visited, path))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasCycleUtil(
        Type node,
        Dictionary<Type, HashSet<Type>> dependencyGraph,
        HashSet<Type> visited,
        HashSet<Type> path)
    {
        visited.Add(node);
        path.Add(node);

        if (dependencyGraph.TryGetValue(node, out var value))
        {
            foreach (var dependency in value)
            {
                if (!visited.Contains(dependency) && HasCycleUtil(dependency, dependencyGraph, visited, path))
                {
                    return true;
                }
                else if (path.Contains(dependency))
                {
                    return true; // Cycle detected
                }
            }
        }

        path.Remove(node);

        return false;
    }

    public IEnumerable<Type> TopologicalSort(Dictionary<Type, HashSet<Type>> dependencyGraph)
    {
        var visited = new HashSet<Type>();
        var result = new List<Type>();

        foreach (var node in dependencyGraph.Keys)
        {
            Visit(node, dependencyGraph, visited, result);
        }

        // we need it reversed to start with the root
        result.Reverse();

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