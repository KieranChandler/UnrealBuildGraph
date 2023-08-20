// See https://aka.ms/new-console-template for more information

using System.Text.Json.Nodes;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using UnrealBuildGraph;

await new Parser().ParseArguments<CliArgs>(args)
    .WithParsedAsync(x =>
    {
        var command = x.Command switch
        {
            "dt" => Command.DependencyTree,
            "up" => Command.UnnecessaryPlugins,
            _ => throw new ArgumentOutOfRangeException(nameof(x.Command), x.Command)
        };
        return new Loop().RunAsync();
    });

public class CliArgs
{
    [Option('c', "command", Required = true)]
    public string Command { get; set; }

    [Option('a', "args", Required = true)] public string CommandArgs { get; set; }
}

public enum Command
{
    Unknown = 0,
    DependencyTree,
    UnnecessaryPlugins
}

public class Loop
{
    public async Task RunAsync()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<GetPluginDependencyTreeQuery>()
            .BuildServiceProvider();

        const string sourceEnvVarKey = "UE4_SOURCE_DIRECTORY";

        var sourceDir = Environment.GetEnvironmentVariable(sourceEnvVarKey);
        if (sourceDir is null)
            throw new InvalidOperationException(sourceEnvVarKey);

        // TODO: Handle case sensitivity e.g. *.Build.cs vs *.build.cs
        var pluginFileNames = Directory.GetFiles(sourceDir, "*.uplugin", SearchOption.AllDirectories);
        var moduleFileNames = Directory.GetFiles(sourceDir, "*.Build.cs", SearchOption.AllDirectories);
        Console.WriteLine($"Found {pluginFileNames.Length} plugins");
        Console.WriteLine($"Found {moduleFileNames.Length} modules");

        var pluginParseTasks = pluginFileNames.Select(x => UPlugin.CreateAsync(Path.GetFileNameWithoutExtension(x), x));

        var uPlugins = await Task.WhenAll(pluginParseTasks);

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
    }
}