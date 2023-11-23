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

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures.Attributes
{
    /// <summary>
    /// Use this in combination with <see cref="Orderers.ScenarioStepOrderer"/> to execute xUnit test cases in
    /// order according to step number. Lowest numbers are executed first.
    ///
    /// Inspired by: https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit#order-by-custom-attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ScenarioStepAttribute : Attribute
    {
        public ScenarioStepAttribute(int number)
        {
            Number = number;
        }

        public int Number { get; }
    }
}
