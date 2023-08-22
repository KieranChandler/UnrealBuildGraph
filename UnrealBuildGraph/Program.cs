// See https://aka.ms/new-console-template for more information

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using UnrealBuildGraph;

const string sourceEnvVarKey = "UE4_SOURCE_DIRECTORY";

var sourceDir = Environment.GetEnvironmentVariable(sourceEnvVarKey);
if (sourceDir is null)
    throw new InvalidOperationException(sourceEnvVarKey);

var pluginFileNames = Directory.GetFiles(sourceDir, "*.uplugin", SearchOption.AllDirectories);
// TODO: Handle case sensitivity e.g. *.Build.cs vs *.build.cs
// var moduleFileNames = Directory.GetFiles(sourceDir, "*.Build.cs", SearchOption.AllDirectories);
Console.WriteLine($"Found {pluginFileNames.Length} plugins");
// Console.WriteLine($"Found {moduleFileNames.Length} modules");

var pluginParseTasks = pluginFileNames.Select(x => UPlugin.CreateAsync(Path.GetFileNameWithoutExtension(x), x));

var uPlugins = await Task.WhenAll(pluginParseTasks);

var serviceProvider = new ServiceCollection()
    .AddSingleton<GetPluginDependencyTreeQuery>()
    .AddSingleton<GetUnnecessaryPluginsQuery>()
    .BuildServiceProvider();

var curInput = "";
while (curInput != "quit")
{
    if (!string.IsNullOrWhiteSpace(curInput))
    {
        var pluginsByName = uPlugins.ToDictionary(x => x.Name, x => x);
        if (curInput.StartsWith("dt"))
        {
            var commandArgs = curInput.TrimStart('d').TrimStart('t').Trim();
            serviceProvider
                .GetRequiredService<GetPluginDependencyTreeQuery>()
                .Get(pluginsByName, commandArgs);
        }
        else if (curInput.StartsWith("up"))
        {
            var commandArgs = curInput.TrimStart('u').TrimStart('p').Trim();
            await serviceProvider.GetRequiredService<GetUnnecessaryPluginsQuery>()
                .Get(pluginsByName, commandArgs);
        }
    }

    Console.WriteLine("\n\nEnter command:");
    curInput = Console.ReadLine();
}