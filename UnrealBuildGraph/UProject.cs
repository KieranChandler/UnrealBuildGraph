using System.Text.Json;

namespace UnrealBuildGraph;

public record UProject(string Name, string[] Plugins)
{
    public static async Task<UProject> CreateAsync(string fileName)
    {
        var fileStream = File.OpenRead(fileName);
        var jsonDocument = await JsonDocument.ParseAsync(fileStream);
        var pluginNames = jsonDocument.RootElement
            .TryGetProperty("Plugins", out var pluginsProp)
            ? pluginsProp.EnumerateArray()
                .Where(x => x.GetProperty("Enabled").GetBoolean())
                .Select(x => x.GetProperty("Name").GetString())
                .ToArray()
            : Array.Empty<string>();

        return new UProject(
            Path.GetFileNameWithoutExtension(fileName),
            pluginNames);
    }
}