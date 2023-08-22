using FluentAssertions;

namespace UnrealBuildGraph.Tests;

public class GetUnnecessaryPluginsQueryTests
{
    [Fact]
    public async Task FindsUnnecessaryPlugins()
    {
        var query = new GetUnnecessaryPluginsQuery();

        var plugins = new Dictionary<string, UPlugin>
        {
            ["TestPlugin1"] = CreatePlugin("TestPlugin1", new[] { "TestPlugin4" }),
            ["TestPlugin2"] = CreatePlugin("TestPlugin2", Array.Empty<string>()),
            ["TestPlugin3"] = CreatePlugin("TestPlugin3", new[] { "TestPlugin4" }),
            ["TestPlugin4"] = CreatePlugin("TestPlugin4", new[] { "TestPlugin5" }),
            ["TestPlugin5"] = CreatePlugin("TestPlugin5", new[] { "TestPlugin1" }),
            ["TestPlugin6"] = CreatePlugin("TestPlugin6", new[] { "TestPlugin1" }),
            ["UnreferencedEnabledByDefault"] = CreatePlugin("TestPlugin6", new[] { "TestPlugin1" }, true),
        };

        await using var writer = new StringWriter();
        Console.SetOut(writer);

        await query.Get(plugins, "Test.uproject");

        writer.ToString().Should().Be(
"""
TestPlugin2: Plugins/TestPlugin2
TestPlugin6: Plugins/TestPlugin6

""" // trailing newline required
        );
    }

    private static UPlugin CreatePlugin(string name, string[] dependencies, bool enabledByDefault = false)
    {
        return new UPlugin(name, name, enabledByDefault, $"Plugins/{name}", Array.Empty<string>(), dependencies);
    }
}