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

using Xunit.Sdk;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes;

/// <summary>
/// Apply attribute on xUnit test class for subsystem tests to specify in
/// which Azure environment the containing tests should be executed, and if
/// they are executed as part of the 'Deployment' pipeline, 'Scheduled' or both ('Always').
/// Multiple attributes can be applied to specify execution in multiple environments.
///
/// GitHub workflows should then use xUnit trait filter expressions to execute
/// tests accordingly.
///
/// See xUnit filter possibilities: https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=xunit
/// </summary>
[TraitDiscoverer(
    typeName: "Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.TraitDiscoverers.ExecutionContextDiscoverer",
    assemblyName: "Energinet.DataHub.Wholesale.SubsystemTests")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExecutionContextAttribute : Attribute, ITraitAttribute
{
    public ExecutionContextAttribute(AzureEnvironment environment = AzureEnvironment.AllDev, WorkflowTrigger trigger = WorkflowTrigger.Deployment)
    {
    }
}

/// <summary>
/// Azure environment filter.
/// </summary>
public enum AzureEnvironment
{
    AllDev,
    Dev001,
    Dev002,
    Dev003,
}

/// <summary>
/// GitHub workflow trigger filter.
/// </summary>
public enum WorkflowTrigger
{
    Always,
    Deployment,
    Scheduled,
}
