using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DbActivities
{
    public static class AssemblyUtils
    {
        public static string GetInformationalVersion(this Assembly assembly)
            => assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;

        public static ActivitySource CreateActivitySource(this Assembly assembly)
            => new(assembly.GetName().GetAssemblyName() ?? "Unknown assembly name", assembly.GetInformationalVersion());

        public static string GetAssemblyName(this AssemblyName assemblyName)
        {
            return assemblyName.Name ?? throw new Exception("Assembly name is missing");
        }
    }
}