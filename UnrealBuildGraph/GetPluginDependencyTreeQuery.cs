using System.Globalization;

namespace UnrealBuildGraph;

public class GetPluginDependencyTreeQuery
{
    private const char IndentChar = '\t';

    public void Get(IDictionary<string, UPlugin> plugins, string pluginName)
    {
        Console.WriteLine("==============================");
        Console.WriteLine(pluginName);
        DrawDependenciesForPlugin(plugins, pluginName, 1);
        Console.WriteLine("==============================");
    }

    private static void DrawDependenciesForPlugin(
        IDictionary<string, UPlugin> plugins, string pluginName, int indentLevel)
    {
        var indents = "";
        for (var i = 0; i < indentLevel; i++)
        {
            indents += IndentChar;
        }

        var dependentPlugins = plugins
            .Where(x => x.Value.Dependencies.Contains(pluginName, StringComparer.OrdinalIgnoreCase))
            .ToList();

        foreach (var dependentPlugin in dependentPlugins)
        {
            Console.WriteLine(indents + dependentPlugin.Key);
            DrawDependenciesForPlugin(plugins, dependentPlugin.Value.Name, indentLevel + 1);
        }
    }
}