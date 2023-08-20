namespace UnrealBuildGraph;

public class GetUnnecessaryPluginsQuery
{
    public async Task Get(Dictionary<string, UPlugin> plugins, string uProjectPath)
    {
        var project = await UProject.CreateAsync(uProjectPath);

        foreach (var plugin in project.Plugins)
        {
            GetDependentPluginsRecursive(plugins, plugin);
        }

        var necessaryPlugins = GetDependentPluginsRecursive(project.Plugins)
            .Distinct();
    }

    private string[] GetDependentPluginsRecursive(Dictionary<string, UPlugin> plugins, string pluginName)
    {
        var dependentPlugins = plugins
            .Where(x => x.Value.Dependencies.Contains(pluginName, StringComparer.OrdinalIgnoreCase))
            .ToList();

        return dependentPlugins
            .Select(x => x.Key)
            .Concat(
                dependentPlugins.Select(x => GetDependentPluginsRecursive(plugins, x.Key))
            );
    }
}