using System.Reflection;
using FluentAssertions;
using Xunit;

namespace DbActivities.Tests
{
    public class AssemblyExtensionTests
    {
        [Fact]
        public void ShouldGetActivitySource()
        {
            var activitySource = typeof(AssemblyExtensionTests).Assembly.CreateActivitySource();
            activitySource.Name.Should().Be("DbActivities.Tests");
            activitySource.Version.Should().Be("1.0.0");
        }

        [Fact]
        public void ShouldThrowExceptionWhenSimpleAssemblyNameIsMissing()
        {
            var assemblyName = new AssemblyName();
            var exception = Record.Exception(() => assemblyName.GetAssemblySimpleName());
            exception.Message.Should().Be("Assembly name is missing");
        }
    }
}