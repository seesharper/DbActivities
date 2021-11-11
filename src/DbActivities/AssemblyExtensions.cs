using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DbActivities
{
    internal static class AssemblyExtensions
    {
        public static string GetInformationalVersion(this Assembly assembly)
            => assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;

        public static ActivitySource CreateActivitySource(this Assembly assembly)
            => new(assembly.GetName().GetAssemblySimpleName(), assembly.GetInformationalVersion());

        public static string GetAssemblySimpleName(this AssemblyName assemblyName)
        {
            return assemblyName.Name ?? throw new Exception("Assembly name is missing");
        }
    }
}