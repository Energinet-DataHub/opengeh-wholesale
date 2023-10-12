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

namespace Test.Core;

public static class EmbeddedResources
{
    /// <summary>
    /// The document path must be relative to the current .NET project.
    /// Example: <example>Stuff.MyFile.txt</example>.
    /// It is the responsibility of the caller to dispose the stream.
    /// </summary>
    /// <param name="relativeDocumentPath">The relative namespace and document path. Dot-separated.</param>
    /// <typeparam name="TRoot">
    /// A type with the root namespace.
    /// I.e. the <paramref name="relativeDocumentPath"/> is relative to the namespace of this type.</typeparam>
    public static Stream GetStream<TRoot>(string relativeDocumentPath)
    {
        var resourceName = $"{typeof(TRoot).Namespace}.{relativeDocumentPath}";

        var stream = Assembly.GetAssembly(typeof(TRoot))!.GetManifestResourceStream(resourceName);

        return stream
            ?? throw new ArgumentException($"Resource '{resourceName}' not found.");
    }
}
