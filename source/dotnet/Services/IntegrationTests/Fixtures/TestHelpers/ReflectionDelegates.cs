// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Fixtures.TestHelpers;

/// <summary>
/// The class provides delegates/functions that can assist
/// when discovering types with the use of reflection.
/// </summary>
public static class ReflectionDelegates
{
    private static readonly Type _functionAttribute = typeof(FunctionAttribute);

    /// <summary>
    /// Find all types
    /// </summary>
    /// <returns>All types within an assembly</returns>
    public static Func<Type, IEnumerable<Type>> FindAllTypes()
        => t => t.Assembly.GetTypes();

    /// <summary>
    /// Find all types within a collection of assemblies
    /// </summary>
    /// <returns>All types found within the collection of assemblies</returns>
    public static Func<Assembly[], IEnumerable<Type>> FindAllTypesInAssemblies()
        => assemblies => assemblies.SelectMany(t => t.GetTypes());

    /// <summary>
    /// Reduce collection to match all types that have at least one method that contains <see cref="FunctionAttribute"/>
    /// </summary>
    /// <returns>All types filter to contain <see cref="FunctionAttribute"/></returns>
    public static Func<IEnumerable<Type>, IEnumerable<Type>> FindAllFunctionTypes()
    {
        return types => types.Where(type => type.GetMethods().Any(MethodIsAnnotatedWithFunctionAttribute));
    }

    /// <summary>
    /// Map all constructor dependencies per type that have one public constructor
    /// </summary>
    /// <returns>All constructor dependencies</returns>
    public static Func<IEnumerable<Type>, IEnumerable<Type>> FindAllConstructorDependencies()
    {
        return types => types
            .Select(GetOnePublicConstructor)
            .SelectMany(GetConstructorParameters);
    }

    /// <summary>
    /// Find a constructor dependencies for a type
    /// </summary>
    /// <returns>Collection of all constructor dependencies</returns>
    public static Func<Type, IEnumerable<Type>> FindAllConstructorDependenciesForType()
    {
        return type => GetConstructorParameters(GetOnePublicConstructor(type));
    }

    /// <summary>
    /// Find all types that implement a certain type
    /// </summary>
    /// <returns>Collection of all types that implements the given type</returns>
    public static Func<Type, IEnumerable<Type>, IEnumerable<Type>> FindAllTypesThatImplementType()
    {
        return (targetType, types) =>
            types.Where(targetType.IsAssignableFrom);
    }

    /// <summary>
    /// Find all types that implement a generic interface
    /// </summary>
    /// <returns>Collection of the type that implements the certain generic interface</returns>
    public static Func<Type, IEnumerable<Type>, IEnumerable<Type>> FindAllTypesThatImplementGenericInterface()
    {
        return (targetType, types) =>
            types.Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetType));
    }

    private static ConstructorInfo? GetOnePublicConstructor(Type? type)
    {
        return type?.GetConstructors().SingleOrDefault(ci => ci.IsPublic && ci.IsStatic == false);
    }

    private static bool MethodIsAnnotatedWithFunctionAttribute(MethodInfo methodInfo)
    {
        return methodInfo.GetCustomAttributes(_functionAttribute).Any();
    }

    private static IEnumerable<Type> GetConstructorParameters(ConstructorInfo? ci)
    {
        return ci == null ? Array.Empty<Type>() : ci.GetParameters().Select(p => p.ParameterType);
    }
}
