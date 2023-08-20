// See https://aka.ms/new-console-template for more information

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using UnrealBuildGraph;

Console.WriteLine("Hello, World!");

const string sourceEnvVarKey = "UE4_SOURCE_DIRECTORY";

var sourceDir = Environment.GetEnvironmentVariable(sourceEnvVarKey);
if (sourceDir is null)
    throw new InvalidOperationException(sourceEnvVarKey);

// TODO: Handle case sensitivity e.g. *.Build.cs vs *.build.cs
var pluginFileNames = Directory.GetFiles(sourceDir, "*.uplugin", SearchOption.AllDirectories);
var moduleFileNames = Directory.GetFiles(sourceDir, "*.Build.cs", SearchOption.AllDirectories);
Console.WriteLine($"Found {pluginFileNames.Length} plugins");
Console.WriteLine($"Found {moduleFileNames.Length} modules");

// var pluginParseTasks = pluginFileNames.Select(x =>
//     JsonSerializer.DeserializeAsync<UPlugin>(File.OpenRead(x)).AsTask()
// );

var pluginParseTasks = pluginFileNames.Select(x => UPlugin.CreateAsync(Path.GetFileNameWithoutExtension(x), x));

var uPlugins = await Task.WhenAll(pluginParseTasks);

var serviceProvider = new ServiceCollection()
    .AddSingleton<GetPluginDependencyTreeQuery>()
    .BuildServiceProvider();

var curInput = "";
while (curInput != "quit")
{
    if (!string.IsNullOrWhiteSpace(curInput))
    {
        serviceProvider
            .GetRequiredService<GetPluginDependencyTreeQuery>()
            .Get(uPlugins.ToDictionary(x => x.Name, x => x), curInput);
    }

    curInput = Console.ReadLine();
}

