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

using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.TraitDiscoverers;

/// <summary>
/// This class discovers all of the xUnit test classes that have applied
/// the <see cref="ExecutionContextAttribute"/> attribute. It then configures
/// xUnit 'traits' based on the parameters given to the attribute.
/// Traits should be used in filters to control which tests we execute in a given
/// context.
/// </summary>
public class ExecutionContextDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var constructorArgs = traitAttribute
            .GetConstructorArguments()
            .ToList();

#pragma warning disable CS8604 // Possible null reference argument.
        yield return new KeyValuePair<string, string>("ExecutionEnvironment", constructorArgs[0].ToString());
        yield return new KeyValuePair<string, string>("ExecutionTrigger", constructorArgs[1].ToString());
#pragma warning restore CS8604 // Possible null reference argument.
    }
}
