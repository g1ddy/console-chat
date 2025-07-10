using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using RaindropServer.Raindrops;

namespace RaindropServer.Tests;

public class MetadataAttributesTests
{
    [Fact]
    public void Tool_method_parameters_have_descriptions()
    {
        var toolTypes = typeof(RaindropsTools).Assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<ModelContextProtocol.Server.McpServerToolTypeAttribute>() != null);

        foreach (var type in toolTypes)
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttribute<ModelContextProtocol.Server.McpServerToolAttribute>() != null);
            foreach (var method in methods)
            {
                foreach (var param in method.GetParameters())
                {
                    var desc = param.GetCustomAttribute<DescriptionAttribute>();
                    Assert.NotNull(desc);
                    Assert.False(string.IsNullOrWhiteSpace(desc!.Description));
                }
            }
        }
    }

    [Fact]
    public void Record_properties_have_descriptions()
    {
        var recordTypes = GetDescribedRecordTypes(typeof(Raindrop).Assembly);

        foreach (var type in recordTypes)
        {
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var desc = prop.GetCustomAttribute<DescriptionAttribute>();
                Assert.NotNull(desc);
                Assert.False(string.IsNullOrWhiteSpace(desc!.Description));
            }
        }
    }

    private static IEnumerable<Type> GetDescribedRecordTypes(Assembly assembly)
        => assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType
                        && t.Namespace?.Contains("Common") == false
                        && t.GetCustomAttribute<DescriptionAttribute>() != null);
}
