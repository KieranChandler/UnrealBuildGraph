namespace UnrealBuildGraph;

public class GetUnnecessaryPluginsQuery
{
    public async Task Get(Dictionary<string, UPlugin> plugins, string uProjectPath)
    {
        var project = await UProject.CreateAsync(uProjectPath);

        // Exclude plugins that are installed in the project
        var enginePluginsForProject = project.Plugins
            .Where(plugins.ContainsKey)
            .ToArray();

        var necessaryPlugins = new List<string>(
            enginePluginsForProject
                .Concat(plugins
                    .Where(x => x.Value.EnabledByDefault)
                    .Select(x => x.Key))
        );
        var visitedPlugins = new List<string>();
        foreach (var plugin in enginePluginsForProject)
        {
            necessaryPlugins.AddRange(BuildDependenciesRecursive(plugins, plugin, visitedPlugins));
        }

        necessaryPlugins = necessaryPlugins.Distinct().ToList();
        var unnecessaryPlugins = plugins
            .Where(x => !necessaryPlugins.Contains(x.Key))
            .Where(x => !x.Value.EnabledByDefault)
            .OrderBy(x => x.Value.Path);
        foreach (var (pluginName, plugin) in unnecessaryPlugins)
        {
            Console.WriteLine($"{plugin.Name}: {plugin.Path}");
        }
    }

    private static string[] BuildDependenciesRecursive(Dictionary<string, UPlugin> plugins, string pluginName,
        List<string>? visited = null)
    {
        visited ??= new List<string>();
        if (visited.Contains(pluginName))
        {
            return Array.Empty<string>();
        }

        visited.Add(pluginName);

        return new[] { pluginName }
            .Concat(
                plugins[pluginName].Dependencies
                    .Select(x => BuildDependenciesRecursive(plugins, x, visited))
                    .SelectMany(x => x)
            )
            .ToArray();
    }
}