using System.Text.Json;

public record UPlugin(
    string Name, string FriendlyName, bool EnabledByDefault, string Path, string[] Modules, string[] Dependencies)
{
    public static async Task<UPlugin> CreateAsync(string name, string fileName)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true
        };
        var fileStream = File.OpenRead(fileName);
        // var result = await JsonSerializer.DeserializeAsync<UPlugin>(fileStream, jsonSerializerOptions);

        // if (result is null)
        // {
        //     throw new InvalidOperationException("Invalid plugin file");
        // }
        //
        // Console.WriteLine($"Plugin {name} successfully parsed");
        // return result with {Name = name};

        var jsonDocument = await JsonDocument.ParseAsync(fileStream, new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        });
        var friendlyName = jsonDocument.RootElement.GetProperty("FriendlyName").GetString();
        var moduleNames = jsonDocument.RootElement
            .TryGetProperty("Modules", out var modulesProp)
            ? modulesProp.EnumerateArray()
                .Select(x => x.GetProperty("Name").GetString())
                .ToArray()
            : Array.Empty<string>();
        var dependencies = jsonDocument.RootElement
            .TryGetProperty("Plugins", out var pluginsProp)
            ? pluginsProp.EnumerateArray()
                .Where(x => x.GetProperty("Enabled").GetBoolean())
                .Select(x => x.GetProperty("Name").GetString())
                .ToArray()
            : Array.Empty<string>();

        var enabledByDefault = jsonDocument.RootElement
                .TryGetProperty("EnabledByDefault", out var enabledByDefaultProp)
            && enabledByDefaultProp.GetBoolean();

        var directoryName = System.IO.Path.GetDirectoryName(
            System.IO.Path.GetFullPath(fileName));

        return new UPlugin(name, friendlyName, enabledByDefault, directoryName, moduleNames, dependencies);
    }
}