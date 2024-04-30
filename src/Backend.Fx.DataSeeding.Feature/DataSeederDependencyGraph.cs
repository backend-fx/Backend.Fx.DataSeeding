using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.DataSeeding.Feature;

public class DataSeederDependencyGraph : IReadOnlyDictionary<Type, HashSet<Type>>
{
    private readonly ILogger _logger = Log.Create<DataSeederDependencyGraph>();
    private readonly Dictionary<Type, HashSet<Type>> _dependencyGraph;
    private string _cycle;

    public DataSeederDependencyGraph(IEnumerable<IDataSeeder> dataSeeders)
    {
        _dependencyGraph = Build(dataSeeders.ToArray());

        if (HasCycle())
        {
            throw new InvalidOperationException(
                $"Cycle detected in data seeder dependencies: {_cycle} Please check the DependsOn properties of your seeders.");
        }
    }

    private static Dictionary<Type, HashSet<Type>> Build(IDataSeeder[] dataSeeders)
    {
        var dependencyGraph = new Dictionary<Type, HashSet<Type>>();

        // Add all seeders to the graph
        foreach (var seeder in dataSeeders)
        {
            if (!dependencyGraph.ContainsKey(seeder.GetType()))
            {
                dependencyGraph[seeder.GetType()] = new HashSet<Type>();
            }
        }

        foreach (var seeder in dataSeeders)
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

    private bool HasCycle()
    {
        var visited = new HashSet<Type>();
        var path = new HashSet<Type>();

        foreach (var node in Keys)
        {
            if (HasCycleUtil(node, visited, path))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasCycleUtil(
        Type node,
        HashSet<Type> visited,
        HashSet<Type> path)
    {
        visited.Add(node);
        path.Add(node);

        if (TryGetValue(node, out var value))
        {
            foreach (var dependency in value)
            {
                if (!visited.Contains(dependency) && HasCycleUtil(dependency, visited, path))
                {
                    _cycle = dependency.Name + " <- " + string.Join(" <- ", path.Select(t => t.Name));
                    _logger.LogError("Cycle detected: {Cycle}", _cycle);
                    return true;
                }

                if (path.Contains(dependency))
                {
                    return true; // Cycle detected
                }
            }
        }

        path.Remove(node);

        return false;
    }

    public Type[] GetSortedSeederTypes()
    {
        var visited = new HashSet<Type>();
        var result = new List<Type>();

        foreach (var node in Keys)
        {
            Visit(node, visited, result);
        }

        // we need it reversed to start with the root
        result.Reverse();

        return result.ToArray();
    }

    private void Visit(
        Type node,
        HashSet<Type> visited,
        List<Type> result)
    {
        if (visited.Add(node))
        {
            if (TryGetValue(node, out var value))
            {
                foreach (var dependency in value)
                {
                    Visit(dependency, visited, result);
                }
            }

            result.Add(node);
        }
    }

    public IEnumerator<KeyValuePair<Type, HashSet<Type>>> GetEnumerator()
    {
        return _dependencyGraph.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_dependencyGraph).GetEnumerator();
    }

    public int Count => _dependencyGraph.Count;

    public bool ContainsKey(Type key)
    {
        return _dependencyGraph.ContainsKey(key);
    }

    public bool TryGetValue(Type key, out HashSet<Type> value)
    {
        return _dependencyGraph.TryGetValue(key, out value);
    }

    public HashSet<Type> this[Type key] => _dependencyGraph[key];

    public IEnumerable<Type> Keys => ((IReadOnlyDictionary<Type, HashSet<Type>>)_dependencyGraph).Keys;

    public IEnumerable<HashSet<Type>> Values => ((IReadOnlyDictionary<Type, HashSet<Type>>)_dependencyGraph).Values;
}
