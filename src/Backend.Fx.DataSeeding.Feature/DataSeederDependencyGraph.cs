using System.Collections;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.DataSeeding.Feature;

public class DataSeederDependencyGraph : IReadOnlyDictionary<Type, HashSet<Type>>
{
    private readonly ILogger _logger = Log.Create<DataSeederDependencyGraph>();
    private readonly Dictionary<Type, HashSet<Type>> _dependencyGraph;

    public DataSeederDependencyGraph(IEnumerable<IDataSeeder> dataSeeders)
    {
        var seeders = dataSeeders.ToArray();

        ValidateDependencies(seeders);

        _dependencyGraph = Build(seeders);

        if (TryFindCycle(out var cycle))
        {
            throw new InvalidOperationException(
                $"Cycle detected in data seeder dependencies: {cycle}. Please check the DependsOn properties of your seeders."
            );
        }
    }

    private static void ValidateDependencies(IDataSeeder[] seeders)
    {
        var seedersByType = new Dictionary<Type, IDataSeeder>();
        foreach (var seeder in seeders)
        {
            seedersByType[seeder.GetType()] = seeder;
        }

        foreach (var seeder in seeders)
        {
            foreach (var dependency in seeder.DependsOn)
            {
                if (!seedersByType.TryGetValue(dependency, out var dependencySeeder))
                {
                    throw new InvalidOperationException(
                        $"{seeder.GetType().Name} depends on {dependency.Name}, but no such data seeder is registered."
                    );
                }

                // A dependency must run at least as often as its dependent. A seeder runs when its
                // Level is greater than or equal to the application's seeding level, so the dependency's
                // Level must be greater than or equal to the dependent's Level. Otherwise the dependency
                // would be silently skipped while the dependent runs, leaving the data inconsistent.
                if (dependencySeeder.Level < seeder.Level)
                {
                    throw new InvalidOperationException(
                        $"{seeder.GetType().Name} (level {seeder.Level}) depends on {dependency.Name} "
                            + $"(level {dependencySeeder.Level}), but a dependency must run at least as often as its "
                            + $"dependent. Raise the level of {dependency.Name} to at least {seeder.Level}."
                    );
                }
            }
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

    private bool TryFindCycle(out string cycle)
    {
        cycle = string.Empty;
        var visited = new HashSet<Type>();
        var stack = new List<Type>();
        var inStack = new HashSet<Type>();

        foreach (var node in Keys)
        {
            if (!visited.Contains(node) && TryFindCycle(node, visited, stack, inStack, out cycle))
            {
                _logger.LogError("Cycle detected: {Cycle}", cycle);
                return true;
            }
        }

        return false;
    }

    private bool TryFindCycle(
        Type node,
        HashSet<Type> visited,
        List<Type> stack,
        HashSet<Type> inStack,
        out string cycle
    )
    {
        cycle = string.Empty;
        visited.Add(node);
        stack.Add(node);
        inStack.Add(node);

        if (TryGetValue(node, out var dependents))
        {
            foreach (var dependent in dependents)
            {
                if (inStack.Contains(dependent))
                {
                    var startIndex = stack.IndexOf(dependent);
                    cycle = string.Join(
                        " -> ",
                        stack.Skip(startIndex).Append(dependent).Select(t => t.Name)
                    );
                    return true;
                }

                if (
                    !visited.Contains(dependent)
                    && TryFindCycle(dependent, visited, stack, inStack, out cycle)
                )
                {
                    return true;
                }
            }
        }

        stack.RemoveAt(stack.Count - 1);
        inStack.Remove(node);
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

    private void Visit(Type node, HashSet<Type> visited, List<Type> result)
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

    public IEnumerable<Type> Keys =>
        ((IReadOnlyDictionary<Type, HashSet<Type>>)_dependencyGraph).Keys;

    public IEnumerable<HashSet<Type>> Values =>
        ((IReadOnlyDictionary<Type, HashSet<Type>>)_dependencyGraph).Values;
}
