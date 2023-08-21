using FluentAssertions;
using FluentAssertions.Execution;

namespace UnrealBuildGraph.Tests;

public class RecordTests
{
    [Fact]
    public async Task UProjectDeserialization()
    {
        var project = await UProject.CreateAsync("Test.uproject");
        
        using var _ = new AssertionScope();
        project.Name.Should().Be("Test");
        // TestPlugin2 is excluded as it is not enabled
        project.Plugins.Should().BeEquivalentTo("TestPlugin1", "TestPlugin3");
    }
    
    [Fact]
    public async Task UPluginDeserialization()
    {
        var plugin = await UPlugin.CreateAsync("Test", "Test.uplugin");
        
        using var _ = new AssertionScope();
        plugin.Name.Should().Be("Test");
        plugin.FriendlyName.Should().Be("Chaos Cloth");
        plugin.EnabledByDefault.Should().BeTrue();
        plugin.Path.Should().Be(Directory.GetCurrentDirectory());
        plugin.Modules.Should().BeEquivalentTo("TestModule1", "TestModule2");
        // TestPlugin2 is excluded as it is not enabled
        plugin.Dependencies.Should().BeEquivalentTo("TestPlugin1", "TestPlugin3");
    }
}